"""
Generate LaTeX tables from results CSV files.
Creates nicely formatted LaTeX tables that can be included in a thesis.
"""

import pandas as pd
from pathlib import Path

output_dir = Path("Analysis/output")

# Load the CSV files
main_table = pd.read_csv(output_dir / "results_main_table.csv")
full_table = pd.read_csv(output_dir / "results_appendix_full_table.csv")


latex_main = r"""
\begin{table}[h!]
\centering
\caption{Kruskal-Wallis H-test results for the 15 most significant metrics. 
Statistical significance is denoted by asterisks: *** p < 0.001, ** p < 0.01, * p < 0.05.}
\label{tab:significance_main}
\small
\begin{tabular}{lcccccc}
\toprule
\textbf{Metric} & \textbf{H-statistic} & \textbf{p-value} & \textbf{Sig} & 
\textbf{No Bears} & \textbf{Avg Bears} & \textbf{Many Bears} \\
\midrule
"""

# Function to format p-value nicely
def format_p_value(p_val_str):
    """Convert p-value string to nice format"""
    if pd.isna(p_val_str) or p_val_str == "N/A":
        return "N/A"
    try:
        # Extract the numeric value
        p_float = float(p_val_str)
        if p_float < 0.001:
            return "< 0.001"
        elif p_float < 0.01:
            return f"{p_float:.4f}"
        else:
            return f"{p_float:.4f}"
    except:
        return str(p_val_str)

# Add rows
for idx, row in main_table.iterrows():
    metric = row['Metric'].replace('_', '\\_')  # Escape underscores for LaTeX
    h_stat = f"{row['H-statistic']:.4f}" if pd.notna(row['H-statistic']) else "N/A"
    p_val = format_p_value(row['p-value (formatted)'])
    sig = str(row['Significance'])
    
    no_bears = f"{row['No bear mean']:.1f}" if pd.notna(row['No bear mean']) else "N/A"
    avg_bears = f"{row['Average bear mean']:.1f}" if pd.notna(row['Average bear mean']) else "N/A"
    many_bears = f"{row['many bear mean']:.1f}" if pd.notna(row['many bear mean']) else "N/A"
    
    latex_main += f"{metric} & {h_stat} & {p_val} & {sig} & {no_bears} & {avg_bears} & {many_bears} \\\\\n"

latex_main += r"""\bottomrule
\end{tabular}
\end{table}
"""

# Save main table
with open(output_dir / "results_main_table.tex", "w", encoding="utf-8") as f:
    f.write(latex_main)

print("✓ Main table LaTeX generated: results_main_table.tex")

latex_full = r"""\begin{longtable}[]{@{}lcccccc@{}}
\caption{Complete Kruskal-Wallis H-test results for all metrics.}\label{tab:significance_full} \\
\toprule
\textbf{Metric} & \textbf{H-stat} & \textbf{p-value} & \textbf{Sig} & 
\textbf{No Bears} & \textbf{Avg Bears} & \textbf{Many Bears} \\
\midrule
\endfirsthead
\multicolumn{7}{@{}l}{\textit{Table \ref{tab:significance_full} continued}} \\
\toprule
\textbf{Metric} & \textbf{H-stat} & \textbf{p-value} & \textbf{Sig} & 
\textbf{No Bears} & \textbf{Avg Bears} & \textbf{Many Bears} \\
\midrule
\endhead
\midrule
\multicolumn{7}{@{}r@{}\textit{Continued on next page}} \\
\endfoot
\bottomrule
\endlastfoot
"""

# Add all rows
for idx, row in full_table.iterrows():
    metric = row['Metric'].replace('_', '\\_')
    h_stat = f"{row['H-statistic']:.4f}" if pd.notna(row['H-statistic']) else "N/A"
    p_val = format_p_value(row['p-value (formatted)'])
    sig = str(row['Significance'])
    
    no_bears = f"{row['No bear mean']:.1f}" if pd.notna(row['No bear mean']) else "N/A"
    avg_bears = f"{row['Average bear mean']:.1f}" if pd.notna(row['Average bear mean']) else "N/A"
    many_bears = f"{row['many bear mean']:.1f}" if pd.notna(row['many bear mean']) else "N/A"
    
    latex_full += f"{metric} & {h_stat} & {p_val} & {sig} & {no_bears} & {avg_bears} & {many_bears} \\\\\n"

latex_full += r"""\end{longtable}"""

# Save full table
with open(output_dir / "results_appendix_full_table.tex", "w", encoding="utf-8") as f:
    f.write(latex_full)

print("✓ Full table LaTeX generated: results_appendix_full_table.tex")

usage_text = r"""
% ============================================================
% USAGE IN YOUR LaTeX DOCUMENT
% ============================================================

% In your main text (Results section):
\section{Statistical Analysis}
Kruskal-Wallis H-test was performed to identify significant differences 
between scenarios. Table~\ref{tab:significance_main} presents the 15 most 
significant metrics.

\input{Analysis/output/results_main_table}

% In your appendix:
\appendix
\section{Complete Statistical Results}
\input{Analysis/output/results_appendix_full_table}
"""

print("\n" + "="*60)
print("USAGE IN LaTeX:")
print("="*60)
print(usage_text)

print("\nFiles created:")
print(f"  1. {output_dir / 'results_main_table.tex'}")
print(f"  2. {output_dir / 'results_appendix_full_table.tex'}")
print("\nYou can include them with: \\input{Analysis/output/results_main_table}")
