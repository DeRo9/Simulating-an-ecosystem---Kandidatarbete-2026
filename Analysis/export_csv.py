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
		action="store_true",
		help="Infer scenario from filename (e.g. with_bear_run1.csv, no_bear_run1.csv).",
	)
	return parser.parse_args()


def find_csv_files(input_dir: Path, pattern: str) -> list[Path]:
	files = sorted(input_dir.glob(pattern))
	if not files:
		raise FileNotFoundError(f"No CSV files found in {input_dir} with pattern '{pattern}'.")
	return files


def infer_scenario_from_name(filename: str) -> str:
	lower = filename.lower()
	if "no_bear" in lower or "without_bear" in lower:
		return "without_bear"
	if "with_bear" in lower:
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


def main() -> None:
	args = parse_args()
	files = find_csv_files(args.input, args.pattern)
	data = load_runs(files, infer_scenario=args.scenario_from_filename)
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

	print(f"Saved summary + plots to: {args.output.resolve()}")


if __name__ == "__main__":
	main()
