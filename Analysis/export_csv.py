from __future__ import annotations

import argparse
from pathlib import Path
from typing import Iterable

import matplotlib.pyplot as plt
import pandas as pd


NUMERIC_COLUMNS = [
	"BearFinalPopulation",
	"WolfFinalPopulation",
	"MooseFinalPopulation",
	"BearBirths",
	"WolfBirths",
	"MooseBirths",
	"BearDeaths",
	"WolfDeaths",
	"MooseDeaths",
	"BearStarvation",
	"WolfStarvation",
	"MooseStarvation",
	"BearPredation",
	"WolfPredation",
	"MoosePredation",
	"BearPlantMeals",
	"BearAnimalPrey",
	"MoosePlantMeals",
	"WolfCarcass",
	"PacksFormed",
	"PackHuntAttempts",
	"PackHuntSuccess",
	"BearAvgHunger",
	"BearAvgThirst",
	"BearAvgStamina",
	"WolfAvgHunger",
	"WolfAvgThirst",
	"WolfAvgStamina",
	"MooseAvgHunger",
	"MooseAvgThirst",
	"MooseAvgStamina",
	"BearAvgLifespan",
	"WolfAvgLifespan",
	"MooseAvgLifespan",

	#Moose State Time
	"MooseIdleTime",
	"MooseWanderTime",
	"MooseSearchFoodTime",
	"MooseSearchWaterTime",
	"MooseEatTime",
	"MooseDrinkTime",
	"MooseMatingTime",
	"MooseFleeingTime",
	"MooseDefendTime",

	#Bear State Time
	"BearIdleTime",
	"BearWanderTime",
	"BearSearchFoodTime",
	"BearSearchWaterTime",
	"BearEatTime",
	"BearDrinkTime",
	"BearMatingTime",
	"BearFleeingTime",
	"BearDefendTime",
	"BearHuntingTime",

	#Wolf State Time
	"WolfIdleTime",
	"WolfWanderTime",
	"WolfSearchFoodTime",
	"WolfSearchWaterTime",
	"WolfEatTime",
	"WolfDrinkTime",
	"WolfMatingTime",
	"WolfFleeingTime",
	"WolfDefendTime",
	"WolfHuntingTime",

	# Hunt statistics (from StatisticsUI)
	"WolfHuntAttempts",
	"WolfHuntFailures",
	"WolfSuccessfulHunts",
	"BearInterference",
	"BearHuntAttempts",
	"BearHuntFailures",
	"BearSuccessfulHunts",
	"MooseSuccessfulEscape",
]


def parse_args() -> argparse.Namespace:
	parser = argparse.ArgumentParser(
		description="Analyze Unity simulation CSV exports and produce summary + plots."
	)
	parser.add_argument(
		"--input",
		type=Path,
		default=Path("Analysis/data"),
		help="Folder containing simulation_*.csv files.",
	)
	parser.add_argument(
		"--output",
		type=Path,
		default=Path("Analysis/output"),
		help="Folder where summary tables and figures are saved.",
	)
	parser.add_argument(
		"--pattern",
		type=str,
		default="*.csv",
		help="Glob pattern for run files.",
	)
	parser.add_argument(
		"--scenario-from-filename",
		action=argparse.BooleanOptionalAction,
		default=True,
		help="Infer scenario from filename (e.g. with_bear_run1.csv, no_bear_run1.csv, few_bear_run1.csv).",
	)
	return parser.parse_args()


def find_csv_files(input_dir: Path, pattern: str) -> list[Path]:
	files = sorted(input_dir.glob(pattern))
	if not files:
		raise FileNotFoundError(f"No CSV files found in {input_dir} with pattern '{pattern}'.")
	return files


def infer_scenario_from_name(filename: str) -> str:
	lower = filename.lower()

	# Normalize common separators so checks work for names like with-bear-run1.csv.
	for sep in ("-", " "):
		lower = lower.replace(sep, "_")

	if (
		"few_bear" in lower
		or "few_bears" in lower
		or "low_bear" in lower
		or "fa_bjorn" in lower
		or "few" in lower
	):
		return "few_bear"
	if "no_bear" in lower or "without_bear" in lower or "no" in lower:
		return "without_bear"
	if "with_bear" in lower or "many_bear" in lower or "many" in lower:
		return "with_bear"
	return "all"


def load_runs(files: Iterable[Path], infer_scenario: bool) -> pd.DataFrame:
	rows = []
	for idx, path in enumerate(files, start=1):
		df = pd.read_csv(path)
		if df.empty:
			continue

		row = df.iloc[0].copy()
		row["run_id"] = idx
		row["source_file"] = path.name

		if infer_scenario:
			row["scenario"] = infer_scenario_from_name(path.name)
		elif "scenario" not in row:
			row["scenario"] = "all"

		rows.append(row)

	if not rows:
		raise ValueError("CSV files were found but contained no usable rows.")

	data = pd.DataFrame(rows)

	# Force known metrics to numeric where available
	for col in NUMERIC_COLUMNS:
		if col in data.columns:
			data[col] = pd.to_numeric(data[col], errors="coerce")

	return data


def clear_old_plot_images(output_dir: Path) -> None:
	"""Remove previously generated plot images so results from old runs do not linger."""
	for image_path in output_dir.glob("plot_*.png"):
		if image_path.is_file():
			image_path.unlink()


def summarize(data: pd.DataFrame, output_dir: Path) -> None:
	output_dir.mkdir(parents=True, exist_ok=True)

	metric_cols = [c for c in NUMERIC_COLUMNS if c in data.columns]
	if not metric_cols:
		raise ValueError("None of the expected metrics were found in the input CSV files.")

	overall = data[metric_cols].agg(["mean", "std", "min", "max"])
	overall.to_csv(output_dir / "summary_overall.csv")

	by_scenario = (
		data.groupby("scenario", dropna=False)[metric_cols]
		.agg(["mean", "std", "min", "max"])
		.sort_index()
	)
	by_scenario.to_csv(output_dir / "summary_by_scenario.csv")

	data.to_csv(output_dir / "all_runs_combined.csv", index=False)


def plot_population(data: pd.DataFrame, output_dir: Path) -> None:
	cols = ["BearFinalPopulation", "WolfFinalPopulation", "MooseFinalPopulation"]
	cols = [c for c in cols if c in data.columns]
	if not cols:
		return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	stds = grouped.std().fillna(0)

	ax = means.plot(kind="bar", yerr=stds, capsize=4, figsize=(10, 6))
	ax.set_title("Final Population by Scenario")
	ax.set_ylabel("Count")
	ax.set_xlabel("Scenario")
	ax.legend(title="Species")
	ax.set_xticklabels(ax.get_xticklabels(), rotation=0, ha="center")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_final_population.png", dpi=180)
	plt.close()


def plot_deaths(data: pd.DataFrame, output_dir: Path) -> None:
	cols = ["BearDeaths", "WolfDeaths", "MooseDeaths"]
	cols = [c for c in cols if c in data.columns]
	if not cols:
		return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	stds = grouped.std().fillna(0)

	ax = means.plot(kind="bar", yerr=stds, capsize=4, figsize=(10, 6), color=["#8c564b", "#1f77b4", "#2ca02c"])
	ax.set_title("Deaths by Scenario")
	ax.set_ylabel("Count")
	ax.set_xlabel("Scenario")
	ax.legend(title="Species")
	ax.set_xticklabels(ax.get_xticklabels(), rotation=0, ha="center")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_deaths.png", dpi=180)
	plt.close()


def plot_lifespan(data: pd.DataFrame, output_dir: Path) -> None:
	cols = ["BearAvgLifespan", "WolfAvgLifespan", "MooseAvgLifespan"]
	cols = [c for c in cols if c in data.columns]
	if not cols:
		return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	stds = grouped.std().fillna(0)

	ax = means.plot(kind="bar", yerr=stds, capsize=4, figsize=(10, 6))
	ax.set_title("Average Lifespan by Scenario")
	ax.set_ylabel("Age Units")
	ax.set_xlabel("Scenario")
	ax.legend(title="Species")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_avg_lifespan.png", dpi=180)
	plt.close()


def plot_births(data: pd.DataFrame, output_dir: Path) -> None:
	cols = ["BearBirths", "WolfBirths", "MooseBirths"]
	cols = [c for c in cols if c in data.columns]
	if not cols:
		return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	stds = grouped.std().fillna(0)

	ax = means.plot(kind="bar", yerr=stds, capsize=4, figsize=(10, 6))
	ax.set_title("Births by Scenario")
	ax.set_ylabel("Count")
	ax.set_xlabel("Scenario")
	ax.legend(title="Species")
	ax.set_xticklabels(ax.get_xticklabels(), rotation=0, ha="center")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_births.png", dpi=180)
	plt.close()


def plot_starvation(data: pd.DataFrame, output_dir: Path) -> None:
	cols = ["BearStarvation", "WolfStarvation", "MooseStarvation"]
	cols = [c for c in cols if c in data.columns]
	if not cols:
		return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	stds = grouped.std().fillna(0)

	ax = means.plot(kind="bar", yerr=stds, capsize=4, figsize=(10, 6))
	ax.set_title("Deaths by Starvation by Scenario")
	ax.set_ylabel("Count")
	ax.set_xlabel("Scenario")
	ax.legend(title="Species")
	ax.set_xticklabels(ax.get_xticklabels(), rotation=0, ha="center")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_starvation.png", dpi=180)
	plt.close()


def plot_predation(data: pd.DataFrame, output_dir: Path) -> None:
	cols = ["BearPredation", "WolfPredation", "MoosePredation"]
	cols = [c for c in cols if c in data.columns]
	if not cols:
		return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	stds = grouped.std().fillna(0)

	ax = means.plot(kind="bar", yerr=stds, capsize=4, figsize=(10, 6))
	ax.set_title("Deaths by Predation by Scenario")
	ax.set_ylabel("Count")
	ax.set_xlabel("Scenario")
	ax.legend(title="Species")
	ax.set_xticklabels(ax.get_xticklabels(), rotation=0, ha="center")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_predation.png", dpi=180)
	plt.close()


def plot_feeding(data: pd.DataFrame, output_dir: Path) -> None:
	cols = ["BearPlantMeals", "BearAnimalPrey", "MoosePlantMeals", "WolfCarcass"]
	cols = [c for c in cols if c in data.columns]
	if not cols:
		return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	stds = grouped.std().fillna(0)

	ax = means.plot(kind="bar", yerr=stds, capsize=4, figsize=(10, 6))
	ax.set_title("Feeding by Scenario")
	ax.set_ylabel("Count")
	ax.set_xlabel("Scenario")
	ax.legend(title="Food source")
	ax.set_xticklabels(ax.get_xticklabels(), rotation=0, ha="center")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_feeding.png", dpi=180)
	plt.close()


def plot_pack_behavior(data: pd.DataFrame, output_dir: Path) -> None:
	cols = ["PacksFormed", "PackHuntAttempts", "PackHuntSuccess"]
	cols = [c for c in cols if c in data.columns]
	if not cols:
		return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	stds = grouped.std().fillna(0)

	ax = means.plot(kind="bar", yerr=stds, capsize=4, figsize=(10, 6))
	ax.set_title("Pack Behavior by Scenario")
	ax.set_ylabel("Count")
	ax.set_xlabel("Scenario")
	ax.legend(title="Metric")
	ax.set_xticklabels(ax.get_xticklabels(), rotation=0, ha="center")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_pack_behavior.png", dpi=180)
	plt.close()


def plot_avg_needs(data: pd.DataFrame, output_dir: Path) -> None:
	cols = [
		"BearAvgHunger", "BearAvgThirst", "BearAvgStamina",
		"WolfAvgHunger", "WolfAvgThirst", "WolfAvgStamina",
		"MooseAvgHunger", "MooseAvgThirst", "MooseAvgStamina",
	]
	cols = [c for c in cols if c in data.columns]
	if not cols:
		return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	stds = grouped.std().fillna(0)

	ax = means.plot(kind="bar", yerr=stds, capsize=4, figsize=(14, 6))
	ax.set_title("Average Needs at End of Simulation by Scenario (%)")
	ax.set_ylabel("Value (%)")
	ax.set_xlabel("Scenario")
	ax.legend(title="Metric", bbox_to_anchor=(1.01, 1), loc="upper left")
	ax.set_xticklabels(ax.get_xticklabels(), rotation=0, ha="center")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_avg_needs.png", dpi=180)
	plt.close()

def plot_wolf_hunt(data: pd.DataFrame, output_dir: Path) -> None:
	cols = ["WolfHuntAttempts", "WolfHuntFailures", "WolfSuccessfulHunts"]
	cols = [c for c in cols if c in data.columns]
	if not cols:
		return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	stds = grouped.std().fillna(0)

	ax = means.plot(kind="bar", yerr=stds, capsize=4, figsize=(10, 6))
	ax.set_title("Wolf Hunt Statistics by Scenario")
	ax.set_ylabel("Count")
	ax.set_xlabel("Scenario")
	ax.legend(title="Metric")
	ax.set_xticklabels(ax.get_xticklabels(), rotation=0, ha="center")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_wolf_hunt.png", dpi=180)
	plt.close()


def plot_bear_hunt(data: pd.DataFrame, output_dir: Path) -> None:
	cols = ["BearHuntAttempts", "BearHuntFailures", "BearSuccessfulHunts", "BearInterference"]
	cols = [c for c in cols if c in data.columns]
	if not cols:
		return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	stds = grouped.std().fillna(0)

	ax = means.plot(kind="bar", yerr=stds, capsize=4, figsize=(10, 6))
	ax.set_title("Bear Hunt Statistics by Scenario")
	ax.set_ylabel("Count")
	ax.set_xlabel("Scenario")
	ax.legend(title="Metric")
	ax.set_xticklabels(ax.get_xticklabels(), rotation=0, ha="center")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_bear_hunt.png", dpi=180)
	plt.close()


def plot_moose_escape(data: pd.DataFrame, output_dir: Path) -> None:
	cols = ["MooseSuccessfulEscape"]
	cols = [c for c in cols if c in data.columns]
	if not cols:
		return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	stds = grouped.std().fillna(0)

	ax = means.plot(kind="bar", yerr=stds, capsize=4, figsize=(10, 6), color=["#2ca02c"])
	ax.set_title("Moose Successful Escapes by Scenario")
	ax.set_ylabel("Count")
	ax.set_xlabel("Scenario")
	ax.legend(title="Metric")
	ax.set_xticklabels(ax.get_xticklabels(), rotation=0, ha="center")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_moose_escape.png", dpi=180)
	plt.close()


def plot_state_distribution_pie(data: pd.DataFrame, output_dir: Path) -> None:
	"""Plot state distribution as pie charts (proportion of time in each state)."""
	
	scenarios = data["scenario"].unique()
	species_info = [
		("Moose", [col for col in data.columns if col.startswith("Moose") and col.endswith("Time")]),
		("Bear", [col for col in data.columns if col.startswith("Bear") and col.endswith("Time")]),
		("Wolf", [col for col in data.columns if col.startswith("Wolf") and col.endswith("Time")]),
	]
	
	for scenario in scenarios:
		scenario_data = data[data["scenario"] == scenario]
		
		fig, axes = plt.subplots(1, 3, figsize=(18, 5))
		fig.suptitle(f"State Distribution by Species - Scenario: {scenario}", fontsize=14, fontweight="bold")
		
		for idx, (species, state_cols) in enumerate(species_info):
			if not state_cols:
				axes[idx].text(0.5, 0.5, f"No {species} data", ha="center", va="center")
				continue
			
			state_values = scenario_data[state_cols].mean()
			total = state_values.sum()

			if total == 0:
				continue

			state_values = state_values / total * 100
			
			state_labels = [col.replace(species, "").replace("Time", "") for col in state_cols]
			
			filtered_values = [(label, val) for label, val in zip(state_labels, state_values) if val > 0.1]
			if not filtered_values:
				axes[idx].text(0.5, 0.5, f"No significant {species} data", ha="center", va="center")
				continue
			
			labels, values = zip(*filtered_values)
			colors = plt.cm.Set3(range(len(labels)))
			
			wedges, texts, autotexts = axes[idx].pie(
				values, 
				labels=labels, 
				autopct="%1.1f%%", 
				startangle=90,
				colors=colors
			)
			axes[idx].set_title(f"{species}")
			
			for text in texts:
				text.set_fontsize(9)
			for autotext in autotexts:
				autotext.set_fontsize(8)
				autotext.set_color("black")
		
		plt.tight_layout()
		safe_scenario = scenario.replace("/", "_").replace(" ", "_")
		plt.savefig(output_dir / f"plot_state_distribution_{safe_scenario}.png", dpi=180)
		plt.close()

def main() -> None:
	args = parse_args()
	files = find_csv_files(args.input, args.pattern)
	data = load_runs(files, infer_scenario=args.scenario_from_filename)
	args.output.mkdir(parents=True, exist_ok=True)
	clear_old_plot_images(args.output)
	summarize(data, args.output)
	plot_population(data, args.output)
	plot_births(data, args.output)
	plot_deaths(data, args.output)
	plot_starvation(data, args.output)
	plot_predation(data, args.output)
	plot_feeding(data, args.output)
	plot_pack_behavior(data, args.output)
	plot_avg_needs(data, args.output)
	plot_lifespan(data, args.output)
	plot_wolf_hunt(data, args.output)
	plot_bear_hunt(data, args.output)
	plot_moose_escape(data, args.output)
	plot_state_distribution_pie(data, args.output)
	print(f"Saved summary + plots to: {args.output.resolve()}")


if __name__ == "__main__":
	main()
