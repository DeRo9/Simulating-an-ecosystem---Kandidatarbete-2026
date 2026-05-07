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

SPECIES_COLORS = {
    "Bear": "#8B4513",  
    "Wolf": "#2C3E50",
    "Moose": "#556B2F"   
}

PALETTE = {
    "BearPlantMeals": "#FFD700",
    "BearAnimalPrey": "#DC143C",
    "MoosePlantMeals": "#90EE90",
    "WolfCarcass": "#8B0000",
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
                ax.bar_label(container, fmt='%.1f', padding=3, fontsize=15)
    
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
	setup_plot_style()
	cols = ["BearFinalPopulation", "WolfFinalPopulation", "MooseFinalPopulation"]
	cols = [c for c in cols if c in data.columns]
	if not cols:
		return
 
	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	stds = grouped.std().fillna(0)
 
	fig, ax = plt.subplots(figsize=(14, 8))
	x = range(len(means.index))
	width = 0.25
	
	species_list = ["Bear", "Wolf", "Moose"]
		
	for i, col in enumerate(cols):
		species = col.replace("FinalPopulation", "")
		color = SPECIES_COLORS.get(species, "#cccccc")
		positions = [p + width*i for p in x]
		
		bars = ax.bar(positions, means[col], width, 
			label=species, yerr=stds[col], capsize=5, 
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
	stds = grouped.std().fillna(0)

	fig, ax = plt.subplots(figsize=(14, 8))
	x = range(len(means.index))
	width = 0.25

	for i, col in enumerate(cols):
		species = col.replace("Deaths", "")
		color = get_bar_color(col)
		positions = [p + width*i for p in x]
		
		ax.bar(positions, means[col], width, 
			label=species, yerr=stds[col], capsize=5, 
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
		stds = grouped.std().fillna(0)

		fig, ax = plt.subplots(figsize=(14, 8))
		x = range(len(means.index))
		width = 0.25
		
		for i, col in enumerate(cols):
			species = col.replace("Deaths", "")
			color = get_bar_color(col)
			positions = [p + width*i for p in x]
			
			ax.bar(positions, means[col], width, 
				label=species, yerr=stds[col], capsize=5, 
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
	stds = grouped.std().fillna(0)

	fig, ax = plt.subplots(figsize=(14, 8))
	x = range(len(means.index))
	width = 0.25
		
	for i, col in enumerate(cols):
		species = col.replace("Births", "")
		color = get_bar_color(col)
		positions = [p + width*i for p in x]
		
		ax.bar(positions, means[col], width, 
			label=species, yerr=stds[col], capsize=5, 
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
	stds = grouped.std().fillna(0)

	fig, ax = plt.subplots(figsize=(14, 8))
	x = range(len(means.index))
	width = 0.25
		
	for i, col in enumerate(cols):
		species = col.replace("Starvation", "")
		color = get_bar_color(col)
		positions = [p + width*i for p in x]	
		ax.bar(positions, means[col], width, 
			label=species, yerr=stds[col], capsize=5, 
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
		stds = grouped.std().fillna(0)

		fig, ax = plt.subplots(figsize=(14, 8))
		x = range(len(means.index))
		width = 0.25
		
		for i, col in enumerate(cols):
			species = col.replace("Predation", "")
			color = get_bar_color(col)
			positions = [p + width*i for p in x]
			
			ax.bar(positions, means[col], width, 
				label=species, yerr=stds[col], capsize=5, 
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
	cols = ["BearPlantMeals", "BearAnimalPrey", "MoosePlantMeals", "WolfCarcass"]
	cols = [c for c in cols if c in data.columns]
	if not cols: return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	stds = grouped.std().fillna(0)

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
		ax.bar(positions, means[col], width, label=col, yerr=stds[col], 
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
	stds = grouped.std().fillna(0)

	fig, ax = plt.subplots(figsize=(14, 8))
	x = range(len(means.index))
	width = 0.25
	short_labels = ["Packs", "Attempts", "Success"]
		
	for i, col in enumerate(cols):
		color = get_bar_color(col)
		positions = [p + width*i for p in x]
		
		ax.bar(positions, means[col], width, label=col, yerr=stds[col], 
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
	stds = grouped.std().fillna(0)

	fig, ax = plt.subplots(figsize=(18, 9))
	x = range(len(means.index))
	width = 0.09
		
	for i, col in enumerate(cols):
		species_init = col[0] 
		need = col.replace("Avg", "")[4:]
		short_label = f"{species_init}-{need}"
		
		color = get_bar_color(col)
		positions = [p + width*i for p in x]
		
		ax.bar(positions, means[col], width, yerr=stds[col], color=color, alpha=0.8, edgecolor='black')
			
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
	cols = ["WolfHuntAttempts", "WolfHuntFailures", "WolfSuccessfulHunts"]
	cols = [c for c in cols if c in data.columns]
	if not cols: return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	stds = grouped.std().fillna(0)

	fig, ax = plt.subplots(figsize=(14, 8))
	x = range(len(means.index))
	width = 0.25
	labels = ["Attempts", "Failures", "Success"]
		
	for i, col in enumerate(cols):
		colors = [SPECIES_COLORS["Wolf"], "#DC143C", "#20B2AA"]
		positions = [p + width*i for p in x]
			
		ax.bar(positions, means[col], width, label=labels[i], yerr=stds[col], 
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
	cols = ["BearHuntAttempts", "BearHuntFailures", "BearSuccessfulHunts", "BearInterference"]
	cols = [c for c in cols if c in data.columns]
	if not cols: return

	grouped = data.groupby("scenario")[cols]
	means = grouped.mean()
	stds = grouped.std().fillna(0)

	fig, ax = plt.subplots(figsize=(15, 8))
	x = range(len(means.index))
	width = 0.20
	labels = ["Attempts", "Failures", "Success", "Interference"]
		
	for i, col in enumerate(cols):
		color = get_bar_color(col)
		positions = [p + width*i for p in x]
		
		ax.bar(positions, means[col], width, label=labels[i], yerr=stds[col], 
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
	stds = grouped.std().fillna(0)

	fig, ax = plt.subplots(figsize=(10, 8))
	x = range(len(means.index))
	width = 0.4
		
	color = SPECIES_COLORS["Moose"]
	ax.bar(x, means[cols[0]], width, yerr=stds[cols[0]], 
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
	plt.show()
	plt.close()

STATE_COLORS = {
	"SearchFood": "#FFD700",  
    "SearchWater": "#4169E1",  
    "SearchMate": "#FF69B4",  
    "Eat": "#228B22",        
    "Drink": "#1E90FF",      
    "Hunt": "#8B0000",     
    "Defend": "#DC143C",     
    "Fleeing": "#FF4500",   
    "Idle": "#808080",     
    "Wander": "#87CEEB",  
    "Hibernate": "#4B0082",   
    "Mating": "#FF1493",     
}

def plot_state_distribution_pie(data: pd.DataFrame, output_dir: Path) -> None:
    """Plot state distribution with shared legend and hidden small labels to prevent overlap."""
    
    scenarios = data["scenario"].unique()
    species_info = [
        ("Moose", [col for col in data.columns if col.startswith("Moose") and col.endswith("Time")]),
        ("Bear", [col for col in data.columns if col.startswith("Bear") and col.endswith("Time")]),
        ("Wolf", [col for col in data.columns if col.startswith("Wolf") and col.endswith("Time")]),
    ]
    
    for scenario in scenarios:
        scenario_data = data[data["scenario"] == scenario]
        
        fig, axes = plt.subplots(1, 3, figsize=(18, 7))
        fig.suptitle(f"State Distribution by Species - Scenario: {scenario}", fontsize=14, fontweight="bold")
        
        global_handles = []
        global_labels = []

        for idx, (species, state_cols) in enumerate(species_info):
            if not state_cols:
                axes[idx].text(0.5, 0.5, f"No {species} data", ha="center", va="center")
                continue
            
            state_values = scenario_data[state_cols].mean()
            total = state_values.sum()
            if total == 0:
                continue

            state_pcts = (state_values / total * 100)
            state_names = [col.replace(species, "").replace("Time", "") for col in state_cols]
            
            plot_data = []
            for name, val in zip(state_names, state_pcts):
                if val > 0.1: 
                    display_label = name if val >= 1.0 else "" 
                    plot_data.append((name, display_label, val))

            if not plot_data:
                axes[idx].text(0.5, 0.5, f"No significant {species} data", ha="center", va="center")
                continue
                
            original_names, display_labels, values = zip(*plot_data)
            colors = [STATE_COLORS.get(name, "#cccccc") for name in original_names]
                
            wedges, texts, autotexts = axes[idx].pie(
                values,
                labels=display_labels,
                autopct=lambda p: f"{p:.1f}%" if p >= 4 else "",
                startangle=90,
                colors=colors,
                pctdistance=0.75,
                labeldistance=1.1
            )

            axes[idx].set_title(f"{species}")

            if len(original_names) > len(global_labels):
                global_handles = wedges
                global_labels = original_names

            for autotext in autotexts:
                autotext.set_fontsize(8)
                autotext.set_fontweight("bold")
        
        if global_handles:
            fig.legend(
                global_handles, 
                global_labels,
                title="States",
                loc="lower center",
                bbox_to_anchor=(0.5, 0.02),
                ncol=min(len(global_labels), 8),
                fontsize=9
            )
                
        plt.tight_layout(rect=[0, 0.1, 1, 0.95])
        
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
