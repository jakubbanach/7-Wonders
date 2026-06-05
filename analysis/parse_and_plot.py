import re
import os
import sys
from pathlib import Path
import pandas as pd
import numpy as np
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
import seaborn as sns


def parse_results(text: str):
    sections = [s.strip() for s in text.split('--------------------------------------------------')]
    summaries = []
    types_list = []
    for sec in sections:
        if not sec:
            continue
        lines = [l.strip() for l in sec.splitlines() if l.strip()]
        header = lines[0]
        m = re.match(r'Running simulation:\s*(.+)\s+vs\s+(.+)', header)
        if not m:
            continue
        agent1_name = m.group(1).strip()
        agent2_name = m.group(2).strip()
        text_block = '\n'.join(lines[1:])

        def find_first(regex, default=None):
            mm = re.search(regex, text_block)
            return mm.group(1) if mm else default

        total_games = int(find_first(r'Total games:\s*(\d+)', '0'))

        a1_max = find_first(r'Agent1 max points:\s*(\d+),\s*min points:\s*(\d+)', None)
        if a1_max:
            a1_max_val = int(re.search(r'Agent1 max points:\s*(\d+)', text_block).group(1))
            a1_min_val = int(re.search(r'Agent1 max points:\s*(\d+),\s*min points:\s*(\d+)', text_block).group(2))
        else:
            a1_max_val = a1_min_val = None

        a2_max = find_first(r'Agent2 max points:\s*(\d+),\s*min points:\s*(\d+)', None)
        if a2_max:
            a2_max_val = int(re.search(r'Agent2 max points:\s*(\d+)', text_block).group(1))
            a2_min_val = int(re.search(r'Agent2 max points:\s*(\d+),\s*min points:\s*(\d+)', text_block).group(2))
        else:
            a2_max_val = a2_min_val = None

        def parse_win_line(prefix):
            mm = re.search(rf'{prefix}\s*wins:\s*(\d+),\s*avg points:\s*([\d,]+)', text_block)
            if mm:
                wins = int(mm.group(1))
                avg = float(mm.group(2).replace(',', '.'))
                return wins, avg
            return None, None

        a1_wins, a1_avg = parse_win_line('Agent1')
        a2_wins, a2_avg = parse_win_line('Agent2')

        # Parse victory types
        types = {}
        if 'Victory types count:' in text_block:
            parts = text_block.split('Victory types count:')[-1].strip()
            for line in parts.splitlines():
                if ':' not in line:
                    continue
                key, val = [p.strip() for p in line.split(':', 1)]
                try:
                    cnt = int(val)
                except ValueError:
                    try:
                        cnt = int(val.replace(',', ''))
                    except Exception:
                        continue
                # key like 'Gracz 2,Punktowe' or 'Remis,Brak'
                who, typ = [k.strip() for k in key.split(',', 1)] if ',' in key else (key, '')
                if who.lower().startswith('gracz'):
                    who_idx = 1 if '1' in who else 2
                    who_key = f'Agent{who_idx}'
                else:
                    who_key = 'Draw'
                types.setdefault((who_key, typ), 0)
                types[(who_key, typ)] += cnt

        # build summary
        summary = {
            'matchup': f"{agent1_name} vs {agent2_name}",
            'agent1_name': agent1_name,
            'agent2_name': agent2_name,
            'total_games': total_games,
            'agent1_wins': a1_wins or 0,
            'agent2_wins': a2_wins or 0,
            'agent1_avg': a1_avg,
            'agent2_avg': a2_avg,
            'agent1_max': a1_max_val,
            'agent1_min': a1_min_val,
            'agent2_max': a2_max_val,
            'agent2_min': a2_min_val,
        }
        summaries.append(summary)

        for ((who_key, typ), cnt) in types.items():
            types_list.append({
                'matchup': summary['matchup'],
                'who': who_key,
                'type': typ if typ else 'Brak',
                'count': cnt,
            })

    return pd.DataFrame(summaries), pd.DataFrame(types_list)


def make_plots(summary_df: pd.DataFrame, types_df: pd.DataFrame, outdir: Path):
    outdir.mkdir(parents=True, exist_ok=True)
    plots_dir = outdir / 'plots'
    plots_dir.mkdir(exist_ok=True)

    # call individual plot functions
    plot_wins_grouped(summary_df, plots_dir)
    plot_types_stacked(types_df, plots_dir)
    plot_types_stacked_by_agent(types_df, plots_dir)
    plot_avg_points(summary_df, plots_dir)


def plot_wins_grouped(summary_df: pd.DataFrame, plots_dir: Path):
    """Grouped bar: number of wins per agent for each matchup"""
    wins_long = summary_df.melt(id_vars=['matchup'], value_vars=['agent1_wins', 'agent2_wins'], var_name='agent', value_name='wins')
    wins_long['agent'] = wins_long['agent'].map({'agent1_wins': 'Agent1', 'agent2_wins': 'Agent2'})
    plt.figure(figsize=(10, 6))
    ax = sns.barplot(data=wins_long, x='matchup', y='wins', hue='agent')
    plt.xticks(rotation=30, ha='right')
    for p in ax.patches:
        h = p.get_height()
        if pd.notna(h) and h != 0:
            ax.annotate(f'{int(h)}', (p.get_x() + p.get_width() / 2., h), ha='center', va='bottom', fontsize=9)
    plt.tight_layout()
    plt.savefig(plots_dir / 'wins_grouped.png')
    plt.close()


def plot_types_stacked(types_df: pd.DataFrame, plots_dir: Path):
    """Stacked bar: distribution of victory types per matchup"""
    if types_df.empty:
        return
    pivot = types_df.pivot_table(index='matchup', columns='type', values='count', aggfunc='sum', fill_value=0)
    ax = pivot.plot(kind='bar', stacked=True, figsize=(10, 6))
    plt.xticks(rotation=30, ha='right')
    for p in ax.patches:
        h = p.get_height()
        if pd.notna(h) and h > 0:
            x = p.get_x() + p.get_width() / 2
            y = p.get_y() + h / 2
            ax.text(x, y, int(h), ha='center', va='center', fontsize=8, color='white')
    plt.tight_layout()
    plt.savefig(plots_dir / 'types_stacked.png')
    plt.close()


def plot_types_stacked_by_agent(types_df: pd.DataFrame, plots_dir: Path):
    """Two-column stacked bars per matchup: Agent1 and Agent2 side-by-side"""
    if types_df.empty:
        return
    df2 = types_df.pivot_table(index=['matchup', 'who'], columns='type', values='count', aggfunc='sum', fill_value=0).reset_index()
    types_order = types_df.groupby('type')['count'].sum().sort_values(ascending=False).index.tolist()
    agent1 = df2[df2['who'] == 'Agent1'].set_index('matchup')
    agent2 = df2[df2['who'] == 'Agent2'].set_index('matchup')
    matchups = sorted(set(agent1.index).union(set(agent2.index)))
    agent1 = agent1.reindex(matchups, fill_value=0)
    agent2 = agent2.reindex(matchups, fill_value=0)
    n = len(matchups)
    x = np.arange(n)
    width = 0.35
    palette = sns.color_palette('tab10', n_colors=max(3, len(types_order)))
    fig, ax = plt.subplots(figsize=(12, 6))
    bottoms1 = np.zeros(n)
    bottoms2 = np.zeros(n)
    for i, typ in enumerate(types_order):
        vals1 = agent1.get(typ, pd.Series([0]*n, index=matchups)).values
        vals2 = agent2.get(typ, pd.Series([0]*n, index=matchups)).values
        color = palette[i % len(palette)]
        bars1 = ax.bar(x - width/2, vals1, width, bottom=bottoms1, label=typ if i == 0 else "", color=color)
        bars2 = ax.bar(x + width/2, vals2, width, bottom=bottoms2, label=typ if i == 0 else "", color=color, alpha=0.9)
        for rect in bars1:
            h = rect.get_height()
            if h > 0:
                ax.text(rect.get_x() + rect.get_width() / 2, rect.get_y() + h / 2, str(int(h)), ha='center', va='center', fontsize=7, color='white')
        for rect in bars2:
            h = rect.get_height()
            if h > 0:
                ax.text(rect.get_x() + rect.get_width() / 2, rect.get_y() + h / 2, str(int(h)), ha='center', va='center', fontsize=7, color='white')
        bottoms1 += vals1
        bottoms2 += vals2
    ax.set_xticks(x)
    ax.set_xticklabels(matchups, rotation=30, ha='right')
    handles = [plt.Rectangle((0,0),1,1, color=palette[i % len(palette)]) for i in range(len(types_order))]
    ax.legend(handles, types_order, title='Type', bbox_to_anchor=(1.02, 1), loc='upper left')
    ax.set_title('Victory types per Agent (Agent1 left, Agent2 right)')
    plt.tight_layout()
    plt.savefig(plots_dir / 'types_stacked_by_agent.png')
    plt.close()


def plot_avg_points(summary_df: pd.DataFrame, plots_dir: Path):
    """Average points with asymmetric error bars and a separate values plot"""
    fig, ax = plt.subplots(figsize=(10, 6))
    x = range(len(summary_df))
    a1_avgs = summary_df['agent1_avg'].fillna(0)
    a2_avgs = summary_df['agent2_avg'].fillna(0)
    a1_err_lower = (a1_avgs - summary_df['agent1_min'].fillna(a1_avgs)).clip(lower=0)
    a1_err_upper = (summary_df['agent1_max'].fillna(a1_avgs) - a1_avgs).clip(lower=0)
    a2_err_lower = (a2_avgs - summary_df['agent2_min'].fillna(a2_avgs)).clip(lower=0)
    a2_err_upper = (summary_df['agent2_max'].fillna(a2_avgs) - a2_avgs).clip(lower=0)
    width = 0.35
    ax.bar([i - width/2 for i in x], a1_avgs, width, yerr=[a1_err_lower, a1_err_upper], capsize=5, label='Agent1')
    ax.bar([i + width/2 for i in x], a2_avgs, width, yerr=[a2_err_lower, a2_err_upper], capsize=5, label='Agent2')
    ax.set_xticks(x)
    ax.set_xticklabels(summary_df['matchup'], rotation=30, ha='right')
    ax.legend()
    plt.tight_layout()
    plt.savefig(plots_dir / 'avg_points_error.png')
    plt.close()
    # annotate avg bars
    fig2, ax2 = plt.subplots(figsize=(10, 6))
    ax2.bar([i - width/2 for i in x], a1_avgs, width, label='Agent1')
    ax2.bar([i + width/2 for i in x], a2_avgs, width, label='Agent2')
    for rect in ax2.patches:
        h = rect.get_height()
        if pd.notna(h) and h != 0:
            ax2.text(rect.get_x() + rect.get_width() / 2, h, f'{h:.2f}', ha='center', va='bottom', fontsize=8)
    ax2.set_xticks(x)
    ax2.set_xticklabels(summary_df['matchup'], rotation=30, ha='right')
    ax2.legend()
    plt.tight_layout()
    plt.savefig(plots_dir / 'avg_points_values.png')
    plt.close()

    # Avg points with errorbars (asymmetric)
    fig, ax = plt.subplots(figsize=(10, 6))
    x = range(len(summary_df))
    a1_avgs = summary_df['agent1_avg'].fillna(0)
    a2_avgs = summary_df['agent2_avg'].fillna(0)
    a1_err_lower = (a1_avgs - summary_df['agent1_min'].fillna(a1_avgs)).clip(lower=0)
    a1_err_upper = (summary_df['agent1_max'].fillna(a1_avgs) - a1_avgs).clip(lower=0)
    a2_err_lower = (a2_avgs - summary_df['agent2_min'].fillna(a2_avgs)).clip(lower=0)
    a2_err_upper = (summary_df['agent2_max'].fillna(a2_avgs) - a2_avgs).clip(lower=0)

    width = 0.35
    bars_a1 = ax.bar([i - width/2 for i in x], a1_avgs, width, yerr=[a1_err_lower, a1_err_upper], capsize=5, label='Agent1')
    bars_a2 = ax.bar([i + width/2 for i in x], a2_avgs, width, yerr=[a2_err_lower, a2_err_upper], capsize=5, label='Agent2')
    ax.set_xticks(x)
    ax.set_xticklabels(summary_df['matchup'], rotation=30, ha='right')
    ax.legend()
    plt.tight_layout()
    plt.savefig(plots_dir / 'avg_points_error.png')
    plt.close()
    # annotate avg bars
    fig2, ax2 = plt.subplots(figsize=(10, 6))
    # recreate simple bars for annotation overlay
    ax2.bar([i - width/2 for i in x], a1_avgs, width, label='Agent1')
    ax2.bar([i + width/2 for i in x], a2_avgs, width, label='Agent2')
    for rect in ax2.patches:
        h = rect.get_height()
        if pd.notna(h) and h != 0:
            ax2.text(rect.get_x() + rect.get_width() / 2, h, f'{h:.2f}', ha='center', va='bottom', fontsize=8)
    ax2.set_xticks(x)
    ax2.set_xticklabels(summary_df['matchup'], rotation=30, ha='right')
    ax2.legend()
    plt.tight_layout()
    plt.savefig(plots_dir / 'avg_points_values.png')
    plt.close()


def main(inp_path: str = None):
    if inp_path is None:
        inp_path = Path('Heurystyczne Genetic vs Personal 2.txt')
    else:
        inp_path = Path(inp_path)
    if not inp_path.exists():
        print('Input file not found:', inp_path)
        return 1

    text = inp_path.read_text(encoding='utf-8')
    summary_df, types_df = parse_results(text)

    outdir = Path('analysis') / 'results'
    outdir.mkdir(parents=True, exist_ok=True)

    summary_df.to_csv(outdir / 'results_summary.csv', index=False)
    types_df.to_csv(outdir / 'results_types.csv', index=False)

    make_plots(summary_df, types_df, outdir)

    print('Saved CSVs and plots to', outdir)
    return 0


if __name__ == '__main__':
    sys.exit(main())
