from __future__ import annotations

import argparse
from pathlib import Path
from typing import Iterable

import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from scipy import stats


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
	"BearCarcassCount",
	"MoosePlantMeals",
	"WolfCarcassCount",
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

SPECIES_COLORS = {
    "Bear": "#8B4513",  
    "Wolf": "#2C3E50",
    "Moose": "#556B2F"   
}

PALETTE = {
    "BearPlantMeals": "#FFD700",
    "BearCarcassCount": "#DC143C",
    "MoosePlantMeals": "#90EE90",
    "WolfCarcassCount": "#8B0000",
    "PacksFormed": "#D8B724",
    "PackHuntAttempts": "#2CB9E4",
    "PackHuntSuccess": "#1CD12B",
	"BearAvgHunger": "#3DA01F",
	"BearAvgThirst": "#1E949C",
	"BearAvgStamina": "#FAD91F",
	"WolfAvgHunger": "#3DA01F",
	"WolfAvgThirst": "#1E949C",
	"WolfAvgStamina": "#FAD91F",
	"MooseAvgHunger": "#3DA01F",
	"MooseAvgThirst": "#1E949C",
	"MooseAvgStamina": "#FAD91F",
	"BearHuntAttempts": "#1FAAFA", 
	"BearHuntFailures": "#FA391F",
	"BearSuccessfulHunts": "#3AE743",
	"BearInterference": "#572EEE"
}

SCENARIO_ORDER = ["No Bears", "Average Bears", "Many Bears"]

def get_bar_color(col_name: str) -> str:
    """Logic to determine color: Species base color first, then Palette, then Grey."""
    if col_name in PALETTE:
        return PALETTE[col_name]
    
    for species, color in SPECIES_COLORS.items():
        if col_name.startswith(species):
            return color
            
    return "#cccccc" # Default fallback

def setup_plot_style():
    """Configure matplotlib for professional appearance."""
    plt.style.use('seaborn-v0_8-darkgrid')
    plt.rcParams.update({
        'figure.facecolor': '#F8F9FA',
        'axes.facecolor': '#FFFFFF',
        'axes.grid': True,
        'grid.color': '#E0E0E0',
        'grid.alpha': 0.3,
        'axes.spines.left': True,
        'axes.spines.bottom': True,
        'axes.spines.right': False,
        'axes.spines.top': False,
        'font.size': 11,
        'axes.labelsize': 12,
        'axes.titlesize': 14,
        'xtick.labelsize': 10,
        'ytick.labelsize': 10,
        'legend.fontsize': 10,
        'font.family': 'sans-serif',
        'font.sans-serif': ['Arial', 'Helvetica'],
    })

def enhance_bar_plot(ax, title, ylabel, xlabel="Scenario", add_values=True):
	"""Apply consistent enhancements to bar plots."""
	ax.set_title(title, fontsize=14, fontweight='bold', pad=20)
	ax.set_ylabel(ylabel, fontsize=12, fontweight='bold')
	ax.set_xlabel(xlabel, fontsize=12, fontweight='bold')
	ax.grid(axis='y', alpha=0.3, linestyle='--')
	ax.set_axisbelow(True)
		
	for spine in ax.spines.values():
		spine.set_edgecolor('#CCCCCC')
		spine.set_linewidth(0.8)
		
	if add_values:
		for container in ax.containers:
			if hasattr(container, 'patches') and len(container.patches) > 0:
				for patch in container.patches:
					height = patch.get_height()
					ax.text(
						patch.get_x() + patch.get_width() / 2,
						height * 0.5,  # Middle of bar
						f'{height:.1f}',
						ha='center', va='center', fontsize=11,
						fontweight='bold', color='white',
						bbox=dict(boxstyle='round,pad=0.3', 
							facecolor='black', alpha=0.3, edgecolor='none')
					)
		
	return ax


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

	if "no" in lower:
		return "No Bears"

	if "average" in lower:
		return "Average Bears"

	if "many" in lower:
		return "Many Bears"


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

    data["scenario"] = pd.Categorical(
        data["scenario"],
        categories=SCENARIO_ORDER,
        ordered=True
    )

    data = data.sort_values("scenario")

    for col in NUMERIC_COLUMNS:
        if col in data.columns:
            data[col] = pd.to_numeric(data[col], errors="coerce")

    return data


def compute_ci95(grouped) -> pd.DataFrame:
	"""Compute 95% confidence interval: 1.96 * std / sqrt(n)"""
	std = grouped.std().fillna(0)
	n = grouped.count()
	return (1.96 * std / np.sqrt(n.clip(lower=1))).fillna(0)


def asymmetric_yerr(means: pd.Series, ci: pd.Series):
	"""Returns [lower, upper] yerr arrays with lower clamped at 0 (no negative bars)."""
	lower = np.minimum(ci.values, means.values)
	upper = ci.values
	return [lower, upper]


def run_significance_tests(data: pd.DataFrame, output_dir: Path) -> None:
	"""Kruskal-Wallis H-test for each metric across scenarios. Saves p-values to CSV."""
	metric_cols = [c for c in NUMERIC_COLUMNS if c in data.columns]
	scenarios = data["scenario"].dropna().unique()

	if len(scenarios) < 2:
		print("Need at least 2 scenarios for significance testing.")
		return

	results = []
	for col in metric_cols:
		groups = [data[data["scenario"] == s][col].dropna().values for s in scenarios]
		groups = [g for g in groups if len(g) > 0]
		if len(groups) < 2:
			continue
		try:
			h_stat, p_value = stats.kruskal(*groups)
			results.append({"metric": col, "H_statistic": round(h_stat, 4), "p_value": round(p_value, 6), "significant (p<0.05)": p_value < 0.05})
		except Exception:
			pass

	results_df = pd.DataFrame(results).sort_values("p_value")
	results_df.to_csv(output_dir / "significance_tests.csv", index=False)
	print(f"Significance tests saved: {len(results_df)} metrics tested.")


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

def parse_population_history(history_str: str) -> list[int]:
	"""Parse semicolon-separated population history from CSV."""
	if pd.isna(history_str) or not history_str:
		return []
	try:
		return [int(x) for x in str(history_str).split(";")]
	except:
		return []
 
 
def plot_species_population(
    data: pd.DataFrame,
    output_dir: Path,
    species_name: str,
    history_col: str,
    color: str,
	) -> None:
		"""
		Creates ONE figure for ONE species with 3 subplots:
		- No Bears
		- Average Bears
		- Many Bears
		"""

		setup_plot_style()

		scenarios = [s for s in SCENARIO_ORDER if s in data["scenario"].unique()]

		if not scenarios:
			return

		fig, axes = plt.subplots(1, len(scenarios), figsize=(18, 6), sharey=True)

		if len(scenarios) == 1:
			axes = [axes]

		fig.suptitle(
			f"{species_name} Population Over Time",
			fontsize=18,
			fontweight="bold",
			y=1.02
		)

		for ax, scenario in zip(axes, scenarios):

			scenario_data = data[data["scenario"] == scenario]

			if history_col not in scenario_data.columns:
				ax.set_title(f"{scenario}\n(No Data)")
				continue

			histories = [
				parse_population_history(h)
				for h in scenario_data[history_col]
			]

			histories = [h for h in histories if len(h) > 0]

			if not histories:
				ax.set_title(f"{scenario}\n(No Data)")
				continue

			max_len = max(len(h) for h in histories)

			padded = []
			for h in histories:
				if len(h) < max_len:
					h = h + [h[-1]] * (max_len - len(h))
				padded.append(h)

			histories_array = np.array(padded)

			mean_pop = histories_array.mean(axis=0)
			std_pop = histories_array.std(axis=0)

			time_steps = np.arange(len(mean_pop))

			ax.plot(
				time_steps,
				mean_pop,
				color=color,
				linewidth=3,
			)

			ax.fill_between(
				time_steps,
				mean_pop - std_pop,
				mean_pop + std_pop,
				color=color,
				alpha=0.2,
			)

			ax.set_title(
				scenario,
				fontsize=14,
				fontweight="bold"
			)

			ax.set_xlabel(
				"Time Step (5s intervals)",
				fontsize=11,
				fontweight="bold"
			)

			ax.grid(True, alpha=0.3)

		axes[0].set_ylabel(
			"Population Count",
			fontsize=11,
			fontweight="bold"
		)

		plt.tight_layout()

		filename = f"plot_population_{species_name.lower()}.png"

		plt.savefig(
			output_dir / filename,
			dpi=300,
			bbox_inches="tight"
		)

		plt.close()

def plot_population_moose(data: pd.DataFrame, output_dir: Path) -> None:
    plot_species_population(
        data,
        output_dir,
        species_name="Moose",
        history_col="MoosePopulationHistory",
        color=SPECIES_COLORS["Moose"],
    )


def plot_population_wolf(data: pd.DataFrame, output_dir: Path) -> None:
    plot_species_population(
        data,
        output_dir,
        species_name="Wolf",
        history_col="WolfPopulationHistory",
        color=SPECIES_COLORS["Wolf"],
    )


def plot_population_bear(data: pd.DataFrame, output_dir: Path) -> None:
    plot_species_population(
        data,
        output_dir,
        species_name="Bear",
        history_col="BearPopulationHistory",
        color=SPECIES_COLORS["Bear"],
    )

def plot_population(data: pd.DataFrame, output_dir: Path) -> None:
	setup_plot_style()
	cols = ["BearFinalPopulation", "WolfFinalPopulation", "MooseFinalPopulation"]
	cols = [c for c in cols if c in data.columns]
	if not cols:
		return
 
	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	cis = compute_ci95(grouped)
 
	fig, ax = plt.subplots(figsize=(14, 8))
	x = range(len(means.index))
	width = 0.25
	
	species_list = ["Bear", "Wolf", "Moose"]
		
	for i, col in enumerate(cols):
		species = col.replace("FinalPopulation", "")
		color = SPECIES_COLORS.get(species, "#cccccc")
		positions = [p + width*i for p in x]
		
		bars = ax.bar(positions, means[col], width, 
			label=species, yerr=asymmetric_yerr(means[col], cis[col]), capsize=5, 
			color=color, alpha=0.85, edgecolor='black', linewidth=1.2)
		
		for pos in positions:
			ax.text(pos, -1.5, species, 
				ha='center', va='top', fontsize=10, fontweight='bold',
				bbox=dict(boxstyle='round,pad=0.3', facecolor=color, alpha=0.3, edgecolor='none'))
		
	ax.margins(y=0.12)
	ax.set_ylim(bottom=-3)
	
	ax.set_xticks([p + width for p in x])
	ax.set_xticklabels(means.index, rotation=0, fontsize=11, fontweight='bold')
	
	enhance_bar_plot(ax, "Final Population by Scenario", "Population Count")
	ax.legend(title="Species", loc='upper left', framealpha=0.95, fontsize=10)
		
	plt.tight_layout()
	plt.savefig(output_dir / "plot_final_population.png", dpi=300, bbox_inches='tight')
	plt.close()


def plot_deaths(data: pd.DataFrame, output_dir: Path) -> None:
	setup_plot_style()
	cols = ["BearDeaths", "WolfDeaths", "MooseDeaths"]
	cols = [c for c in cols if c in data.columns]
	if not cols: return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	cis = compute_ci95(grouped)

	fig, ax = plt.subplots(figsize=(14, 8))
	x = range(len(means.index))
	width = 0.25

	for i, col in enumerate(cols):
		species = col.replace("Deaths", "")
		color = get_bar_color(col)
		positions = [p + width*i for p in x]
		
		ax.bar(positions, means[col], width, 
			label=species, yerr=asymmetric_yerr(means[col], cis[col]), capsize=5, 
			color=color, alpha=0.85, edgecolor='black', linewidth=1.2)
		
		for pos in positions:
			ax.text(pos, -1.0, species, 
					ha='center', va='top', fontsize=10, fontweight='bold',
					bbox=dict(boxstyle='round,pad=0.3', facecolor=color, alpha=0.2, edgecolor='none'))
		
	ax.margins(y=0.15)
	ax.set_ylim(bottom=-2)
		
	ax.set_xticks([p + width for p in x])
	ax.set_xticklabels(means.index, rotation=0, fontsize=11, fontweight='bold')
		
	enhance_bar_plot(ax, "Total Deaths by Scenario", "Death Count")
	ax.legend(title="Species", loc='upper left', framealpha=0.95)
			
	plt.tight_layout()
	plt.savefig(output_dir / "plot_deaths.png", dpi=300, bbox_inches='tight')
	plt.close()


def plot_lifespan(data: pd.DataFrame, output_dir: Path) -> None:
		setup_plot_style()
		cols = ["BearAvgLifespan", "WolfAvgLifespan", "MooseAvgLifespan"]
		cols = [c for c in cols if c in data.columns]
		if not cols: return

		grouped = data.groupby("scenario")[cols]
		means = grouped.mean()
		cis = compute_ci95(grouped)

		fig, ax = plt.subplots(figsize=(14, 8))
		x = range(len(means.index))
		width = 0.25
		
		for i, col in enumerate(cols):
			species = col.replace("AvgLifespan", "")
			color = get_bar_color(col)
			positions = [p + width*i for p in x]
			
			ax.bar(positions, means[col], width, 
				label=species, yerr=asymmetric_yerr(means[col], cis[col]), capsize=5, 
				color=color, alpha=0.85, edgecolor='black', linewidth=1.2)
			
			for pos in positions:
				ax.text(pos, -1.0, species, 
						ha='center', va='top', fontsize=10, fontweight='bold',
						bbox=dict(boxstyle='round,pad=0.3', facecolor=color, alpha=0.2, edgecolor='none'))
			
		ax.margins(y=0.15)
		ax.set_ylim(bottom=-2)
		ax.set_xticks([p + width for p in x])
		ax.set_xticklabels(means.index, rotation=0, fontsize=11, fontweight='bold')
		
		enhance_bar_plot(ax, "Average Lifespan by Scenario", "Age Units")
		ax.legend(title="Species", loc='upper left')
		plt.tight_layout()
		plt.savefig(output_dir / "plot_avg_lifespan.png", dpi=300, bbox_inches='tight')
		plt.close()


def plot_births(data: pd.DataFrame, output_dir: Path) -> None:
	setup_plot_style()
	cols = ["BearBirths", "WolfBirths", "MooseBirths"]
	cols = [c for c in cols if c in data.columns]
	if not cols: return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	cis = compute_ci95(grouped)

	fig, ax = plt.subplots(figsize=(14, 8))
	x = range(len(means.index))
	width = 0.25
		
	for i, col in enumerate(cols):
		species = col.replace("Births", "")
		color = get_bar_color(col)
		positions = [p + width*i for p in x]
		
		ax.bar(positions, means[col], width, 
			label=species, yerr=asymmetric_yerr(means[col], cis[col]), capsize=5, 
			color=color, alpha=0.85, edgecolor='black', linewidth=1.2)
			
		for pos in positions:
			ax.text(pos, -1.0, species, 
					ha='center', va='top', fontsize=10, fontweight='bold',
					bbox=dict(boxstyle='round,pad=0.3', facecolor=color, alpha=0.2, edgecolor='none'))
			
	ax.margins(y=0.15)
	ax.set_ylim(bottom=-2)
	ax.set_xticks([p + width for p in x])
	ax.set_xticklabels(means.index, rotation=0, fontsize=11, fontweight='bold')
		
	enhance_bar_plot(ax, "Total Births by Scenario", "Birth Count")
	ax.legend(title="Species", loc='upper left')
	plt.tight_layout()
	plt.savefig(output_dir / "plot_births.png", dpi=300, bbox_inches='tight')
	plt.close()


def plot_starvation(data: pd.DataFrame, output_dir: Path) -> None:
	setup_plot_style()
	cols = ["BearStarvation", "WolfStarvation", "MooseStarvation"]
	cols = [c for c in cols if c in data.columns]
	if not cols: return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	cis = compute_ci95(grouped)

	fig, ax = plt.subplots(figsize=(14, 8))
	x = range(len(means.index))
	width = 0.25
		
	for i, col in enumerate(cols):
		species = col.replace("Starvation", "")
		color = get_bar_color(col)
		positions = [p + width*i for p in x]	
		ax.bar(positions, means[col], width, 
			label=species, yerr=asymmetric_yerr(means[col], cis[col]), capsize=5, 
			color=color, alpha=0.85, edgecolor='black', linewidth=1.2)	
		for pos in positions:
			ax.text(pos, -0.5, species, 
					ha='center', va='top', fontsize=10, fontweight='bold',
					bbox=dict(boxstyle='round,pad=0.3', facecolor=color, alpha=0.2, edgecolor='none'))
			
	ax.margins(y=0.15)
	ax.set_ylim(bottom=-1)
	ax.set_xticks([p + width for p in x])
	ax.set_xticklabels(means.index, rotation=0, fontsize=11, fontweight='bold')
	enhance_bar_plot(ax, "Deaths by Starvation", "Starvation Count")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_starvation.png", dpi=300, bbox_inches='tight')
	plt.close()


def plot_predation(data: pd.DataFrame, output_dir: Path) -> None:
		setup_plot_style()
		cols = ["BearPredation", "WolfPredation", "MoosePredation"]
		cols = [c for c in cols if c in data.columns]
		if not cols: return

		grouped = data.groupby("scenario")[cols]
		means = grouped.mean()
		cis = compute_ci95(grouped)

		fig, ax = plt.subplots(figsize=(14, 8))
		x = range(len(means.index))
		width = 0.25
		
		for i, col in enumerate(cols):
			species = col.replace("Predation", "")
			color = get_bar_color(col)
			positions = [p + width*i for p in x]
			
			ax.bar(positions, means[col], width, 
				label=species, yerr=asymmetric_yerr(means[col], cis[col]), capsize=5, 
				color=color, alpha=0.85, edgecolor='black', linewidth=1.2)
			
			for pos in positions:
				ax.text(pos, -0.5, species, 
						ha='center', va='top', fontsize=10, fontweight='bold',
						bbox=dict(boxstyle='round,pad=0.3', facecolor=color, alpha=0.2, edgecolor='none'))
			
		ax.margins(y=0.15)
		ax.set_ylim(bottom=-1)
		ax.set_xticks([p + width for p in x])
		ax.set_xticklabels(means.index, rotation=0, fontsize=11, fontweight='bold')
		
		enhance_bar_plot(ax, "Deaths by Predation", "Count")
		plt.tight_layout()
		plt.savefig(output_dir / "plot_predation.png", dpi=300, bbox_inches='tight')
		plt.close()


def plot_feeding(data: pd.DataFrame, output_dir: Path) -> None:
	setup_plot_style()
	cols = ["BearPlantMeals", "BearCarcassCount", "MoosePlantMeals", "WolfCarcassCount"]
	cols = [c for c in cols if c in data.columns]
	if not cols: return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	cis = compute_ci95(grouped)

	fig, ax = plt.subplots(figsize=(15, 8))
	x = range(len(means.index))
	width = 0.20 
		
	display_names = {
		"BearPlantMeals": "Bear (Plant)",
		"BearAnimalPrey": "Bear (Carcass)",
		"MoosePlantMeals": "Moose (Plant)",
		"WolfCarcass": "Wolf (Carcass)"
	}

	for i, col in enumerate(cols):
		color = get_bar_color(col)
		positions = [p + width*i for p in x]	
		ax.bar(positions, means[col], width, label=col, yerr=asymmetric_yerr(means[col], cis[col]), 
			capsize=4, color=color, alpha=0.85, edgecolor='black')
	
		label = display_names.get(col, col)
		for pos in positions:
			ax.text(pos, -2.0, label, ha='center', va='top', fontsize=8, 
					fontweight='bold', bbox=dict(boxstyle='round,pad=0.2', 
					facecolor=color, alpha=0.2, edgecolor='none'))
			
	ax.margins(y=0.15)
	ax.set_ylim(bottom=-5)
	ax.set_xticks([p + width*1.5 for p in x])
	ax.set_xticklabels(means.index, fontweight='bold')
		
	enhance_bar_plot(ax, "Feeding Habits by Scenario", "Meal Count")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_feeding.png", dpi=300)
	plt.close()


def plot_pack_behavior(data: pd.DataFrame, output_dir: Path) -> None:
	setup_plot_style()
	cols = ["PacksFormed", "PackHuntAttempts", "PackHuntSuccess"]
	cols = [c for c in cols if c in data.columns]
	if not cols: return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	cis = compute_ci95(grouped)

	fig, ax = plt.subplots(figsize=(14, 8))
	x = range(len(means.index))
	width = 0.25
	short_labels = ["Packs\nCreated", "Attempts", "Success"]
		
	for i, col in enumerate(cols):
		color = get_bar_color(col)
		positions = [p + width*i for p in x]
		
		ax.bar(positions, means[col], width, label=col, yerr=asymmetric_yerr(means[col], cis[col]), 
			capsize=5, color=color, alpha=0.85, edgecolor='black')
		
		for pos in positions:
			ax.text(pos, -0.5, short_labels[i], ha='center', va='top', fontsize=10, 
					fontweight='bold', bbox=dict(boxstyle='round,pad=0.3', 
					facecolor=color, alpha=0.2, edgecolor='none'))
			
	ax.margins(y=0.15)
	ax.set_ylim(bottom=-1)
	ax.set_xticks([p + width for p in x])
	ax.set_xticklabels(means.index, fontweight='bold')
		
	enhance_bar_plot(ax, "Wolf Pack Behavior", "Count")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_pack_behavior.png", dpi=300)
	plt.close()

def plot_avg_needs(data: pd.DataFrame, output_dir: Path) -> None:
	setup_plot_style()
	cols = [
		"BearAvgHunger", "BearAvgThirst", "BearAvgStamina",
		"WolfAvgHunger", "WolfAvgThirst", "WolfAvgStamina",
		"MooseAvgHunger", "MooseAvgThirst", "MooseAvgStamina",
	]
	cols = [c for c in cols if c in data.columns]
	if not cols: return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	cis = compute_ci95(grouped)

	fig, ax = plt.subplots(figsize=(18, 9))
	x = range(len(means.index))
	width = 0.09
		
	for i, col in enumerate(cols):
		species_init = col[0] 
		need = col.replace("Avg", "")[4:]
		short_label = f"{species_init}-{need}"
		
		color = get_bar_color(col)
		positions = [p + width*i for p in x]
		
		ax.bar(positions, means[col], width, yerr=asymmetric_yerr(means[col], cis[col]), color=color, alpha=0.8, edgecolor='black')
			
		for pos in positions:
			ax.text(pos, -2, short_label, ha='center', va='top', fontsize=7, rotation=45,
					bbox=dict(boxstyle='round,pad=0.1', facecolor=color, alpha=0.1, edgecolor='none'))
			
	ax.set_xticks([p + width*4 for p in x])
	ax.set_xticklabels(means.index, fontweight='bold')
	ax.set_ylim(bottom=-5)
		
	enhance_bar_plot(ax, "Average Needs at Simulation End", "Value (%)")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_avg_needs.png", dpi=300)
	plt.close()

#We kindoff already have this graph plotted beforehand (Most likely unecessary)
def plot_wolf_hunt(data: pd.DataFrame, output_dir: Path) -> None:
	setup_plot_style()
	cols = ["WolfHuntFailures", "WolfSuccessfulHunts"]
	cols = [c for c in cols if c in data.columns]
	if not cols: return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	cis = compute_ci95(grouped)

	fig, ax = plt.subplots(figsize=(14, 8))
	x = range(len(means.index))
	width = 0.25
	labels = ["Failures", "Success"]
		
	for i, col in enumerate(cols):
		colors = ["#DC143C", "#20B2AA"]
		positions = [p + width*i for p in x]
			
		ax.bar(positions, means[col], width, label=labels[i], yerr=asymmetric_yerr(means[col], cis[col]), 
			capsize=5, color=colors[i], alpha=0.85, edgecolor='black')
			
		for pos in positions:
			ax.text(pos, -1.0, labels[i], ha='center', va='top', fontsize=10, 
					fontweight='bold', bbox=dict(boxstyle='round,pad=0.3', 
					facecolor=colors[i], alpha=0.2, edgecolor='none'))
			
	ax.margins(y=0.15)
	ax.set_ylim(bottom=-2)
	ax.set_xticks([p + width for p in x])
	ax.set_xticklabels(means.index, fontweight='bold')
		
	enhance_bar_plot(ax, "Wolf Hunt Statistics", "Count")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_wolf_hunt.png", dpi=300)
	plt.close()


def plot_bear_hunt(data: pd.DataFrame, output_dir: Path) -> None:
	setup_plot_style()
	cols = ["BearHuntFailures", "BearSuccessfulHunts", "BearInterference"]
	cols = [c for c in cols if c in data.columns]
	if not cols: return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	cis = compute_ci95(grouped)

	fig, ax = plt.subplots(figsize=(15, 8))
	x = range(len(means.index))
	width = 0.20
	labels = ["Failures", "Success", "Interference"]
		
	for i, col in enumerate(cols):
		color = get_bar_color(col)
		positions = [p + width*i for p in x]
		
		ax.bar(positions, means[col], width, label=labels[i], yerr=asymmetric_yerr(means[col], cis[col]), 
			capsize=5, color=color, alpha=0.85, edgecolor='black')
		
		for pos in positions:
			ax.text(pos, -1.0, labels[i], ha='center', va='top', fontsize=9, 
					fontweight='bold', bbox=dict(boxstyle='round,pad=0.3', 
					facecolor=color, alpha=0.2, edgecolor='none'))
		
	ax.margins(y=0.15)
	ax.set_ylim(bottom=-2)
	ax.set_xticks([p + width*1.5 for p in x])
	ax.set_xticklabels(means.index, fontweight='bold')
		
	enhance_bar_plot(ax, "Bear Hunt Statistics", "Count")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_bear_hunt.png", dpi=300)
	plt.close()

def plot_moose_escape(data: pd.DataFrame, output_dir: Path) -> None:
	setup_plot_style()
	cols = ["MooseSuccessfulEscape"]
	cols = [c for c in cols if c in data.columns]
	if not cols: return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	cis = compute_ci95(grouped)

	fig, ax = plt.subplots(figsize=(10, 8))
	x = range(len(means.index))
	width = 0.4
		
	color = SPECIES_COLORS["Moose"]
	ax.bar(x, means[cols[0]], width, yerr=cis[cols[0]], 
		capsize=5, color=color, alpha=0.85, edgecolor='black')
	
	for pos in x:
		ax.text(pos, -1.0, "Moose", ha='center', va='top', fontsize=10, 
				fontweight='bold', bbox=dict(boxstyle='round,pad=0.3', 
				facecolor=color, alpha=0.2, edgecolor='none'))

	ax.set_xticks(x)
	ax.set_xticklabels(means.index, fontweight='bold')
	ax.set_ylim(bottom=-2)
		
	enhance_bar_plot(ax, "Moose Successful Escapes", "Escape Count")
	plt.tight_layout()
	plt.savefig(output_dir / "plot_moose_escape.png", dpi=300)
	plt.close()

STATE_COLORS = {
    "SearchFood": "#4CAF50",      # Bright green
    "Eat": "#2E7D32",             # Forest green
    
    "SearchWater": "#29B6F6",     # Bright blue
    "Drink": "#1565C0",           # Deep blue
    
    "SearchMate": "#EC407A",      # Vibrant pink
    "Mating": "#7B1FA2",          # Rich purple
    
    "Hunting": "#F44336",         # Strong red
    "Defend": "#FF6E40",          # Coral red-orange
    "Fleeing": "#FFA726",         # Warm orange
    
    "Idle": "#757575",            # Gray
    "Wander": "#4DB6AC",          # Muted teal
    "Hibernate": "#512DA8"        # Deep purple
}

def plot_species_state_distribution(
	data: pd.DataFrame,
	output_dir: Path,
	species_name: str,
) -> None:
	"""
	Creates ONE figure for ONE species with 3 subplots (one per scenario):
	- No Bears
	- Average Bears
	- Many Bears
	Shows state distribution as pie charts across scenarios.
	"""
	setup_plot_style()
 
	scenarios = [s for s in SCENARIO_ORDER if s in data["scenario"].unique()]
 
	if not scenarios:
		return
 
	# Get state columns for this species
	state_cols = [col for col in data.columns if col.startswith(species_name) and col.endswith("Time")]
	if not state_cols:
		return
 
	fig, axes = plt.subplots(1, len(scenarios), figsize=(18, 6))
 
	if len(scenarios) == 1:
		axes = [axes]
 
	fig.suptitle(
		f"{species_name} State Distribution Over Scenarios",
		fontsize=18,
		fontweight="bold",
		y=1.02
	)
 
	# Collect all state names for reference
	all_state_names = [col.replace(species_name, "").replace("Time", "") for col in state_cols]
	
	# Track which states actually appear in plots (for accurate legend)
	plotted_states = {}
 
	for ax, scenario in zip(axes, scenarios):
 
		scenario_data = data[data["scenario"] == scenario]
 
		if len(scenario_data) == 0:
			ax.set_title(f"{scenario}\n(No Data)")
			ax.axis('off')
			continue
 
		# Calculate mean state times for this scenario
		state_values = scenario_data[state_cols].mean()
		total = state_values.sum()
 
		if total == 0:
			ax.set_title(f"{scenario}\n(No Data)")
			ax.axis('off')
			continue
 
		# Convert to percentages
		state_pcts = (state_values / total * 100)
		
		# Filter out very small values for cleaner pie chart
		plot_data = []
		for name, pct in zip(all_state_names, state_pcts):
			if pct > 0.5:  # Only include states > 0.5%
				display_label = name if pct >= 3.0 else ""  # Only show label if >= 3%
				plot_data.append((name, display_label, pct))
 
		if not plot_data:
			ax.set_title(f"{scenario}\n(No Data)")
			ax.axis('off')
			continue
 
		original_names, display_labels, values = zip(*plot_data)
		colors = [STATE_COLORS.get(name, "#cccccc") for name in original_names]
 
		# Track plotted states with their colors
		for name, color in zip(original_names, colors):
			if name not in plotted_states:
				plotted_states[name] = color
 
		wedges, texts, autotexts = ax.pie(
			values,
			labels=display_labels,
			autopct=lambda p: f"{p:.1f}%" if p >= 4 else "",
			startangle=90,
			colors=colors,
			pctdistance=0.75,
			labeldistance=1.1,
			textprops={'fontsize': 10, 'weight': 'bold'}
		)
 
		ax.set_title(
			scenario,
			fontsize=14,
			fontweight="bold",
			pad=15
		)
 
		for autotext in autotexts:
			autotext.set_fontsize(9)
			autotext.set_fontweight("bold")
			autotext.set_color("white")
 
	# Add shared legend at the bottom with only plotted states and correct colors
	if plotted_states:
		from matplotlib.patches import Patch
		
		# Sort plotted states in the same order as all_state_names for consistency
		legend_names = [s for s in all_state_names if s in plotted_states]
		legend_colors = [plotted_states[s] for s in legend_names]
		
		# Create legend patches with correct colors
		legend_patches = [Patch(facecolor=color, edgecolor='black', linewidth=0.5) 
		                   for color in legend_colors]
		
		fig.legend(
			legend_patches,
			legend_names,
			title="States",
			loc="lower center",
			bbox_to_anchor=(0.5, -0.05),
			ncol=min(len(legend_names), 8),
			fontsize=10,
			framealpha=0.95
		)
 
	plt.tight_layout(rect=[0, 0.08, 1, 0.96])
 
	filename = f"plot_state_distribution_{species_name.lower()}.png"
	plt.savefig(
		output_dir / filename,
		dpi=300,
		bbox_inches="tight"
	)
 
	plt.show()
	plt.close()
 

def plot_state_distribution_moose(data: pd.DataFrame, output_dir: Path) -> None:
	"""Plot Moose state distribution across scenarios."""
	plot_species_state_distribution(data, output_dir, species_name="Moose")
 
 
def plot_state_distribution_bear(data: pd.DataFrame, output_dir: Path) -> None:
	"""Plot Bear state distribution across scenarios."""
	plot_species_state_distribution(data, output_dir, species_name="Bear")
 
 
def plot_state_distribution_wolf(data: pd.DataFrame, output_dir: Path) -> None:
	"""Plot Wolf state distribution across scenarios."""
	plot_species_state_distribution(data, output_dir, species_name="Wolf")


def main() -> None:
	args = parse_args()
	files = find_csv_files(args.input, args.pattern)
	data = load_runs(files, infer_scenario=args.scenario_from_filename)
	args.output.mkdir(parents=True, exist_ok=True)
	clear_old_plot_images(args.output)
	summarize(data, args.output)
	plot_population_moose(data, args.output)
	plot_population_wolf(data, args.output)
	plot_population_bear(data, args.output)
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
	plot_state_distribution_moose(data, args.output)
	plot_state_distribution_bear(data, args.output)
	plot_state_distribution_wolf(data, args.output)
	run_significance_tests(data, args.output)
	print(f"Saved summary + plots to: {args.output.resolve()}")


if __name__ == "__main__":
	main()
