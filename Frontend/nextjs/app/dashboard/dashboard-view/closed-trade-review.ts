import type { ClosedTradeReview, LiveTrade } from "@/types/live-trading";
import type { PaperTradeFill } from "@/types/paper-trading";

interface ClosedTradeHistoryItem {
  id: string;
  source: "paper" | "alpaca";
  direction: "Buy" | "Sell";
  realizedProfitLoss: number;
  closedAt: string;
  hasStopLoss: boolean;
  hasTakeProfit: boolean;
  rewardRiskRatio: number | null;
}

const MINIMUM_MEANINGFUL_REWARD_RISK = 1.2;
const MAX_PATTERN_WINDOW = 6;

export function buildPaperClosedTradeReview(
  fill: PaperTradeFill,
  closedFills: PaperTradeFill[],
  closedLiveTrades: LiveTrade[],
): ClosedTradeReview {
  const history = buildClosedTradeHistory(closedFills, closedLiveTrades);
  const currentTrade = buildPaperHistoryItem(fill);
  return buildClosedTradeReview(currentTrade, history);
}

export function buildLiveClosedTradeReview(
  trade: LiveTrade,
  closedFills: PaperTradeFill[],
  closedLiveTrades: LiveTrade[],
): ClosedTradeReview {
  const history = buildClosedTradeHistory(closedFills, closedLiveTrades);
  const currentTrade = buildLiveHistoryItem(trade);
  return buildClosedTradeReview(currentTrade, history);
}

function buildClosedTradeHistory(
  closedFills: PaperTradeFill[],
  closedLiveTrades: LiveTrade[],
) {
  return [
    ...closedFills.map(buildPaperHistoryItem),
    ...closedLiveTrades.map(buildLiveHistoryItem),
  ].sort((left, right) => Date.parse(right.closedAt) - Date.parse(left.closedAt));
}

function buildPaperHistoryItem(fill: PaperTradeFill): ClosedTradeHistoryItem {
  return {
    id: `paper-${fill.id}`,
    source: "paper",
    direction: fill.direction,
    realizedProfitLoss: fill.realizedProfitLoss,
    closedAt: fill.executedAt,
    hasStopLoss: false,
    hasTakeProfit: false,
    rewardRiskRatio: null,
  };
}

function buildLiveHistoryItem(trade: LiveTrade): ClosedTradeHistoryItem {
  const rewardRiskRatio = getRewardRiskRatio(trade);

  return {
    id: `alpaca-${trade.id}`,
    source: "alpaca",
    direction: trade.direction,
    realizedProfitLoss: trade.realizedProfitLoss,
    closedAt: trade.closedAt ?? trade.executedAt,
    hasStopLoss: trade.stopLoss != null,
    hasTakeProfit: trade.takeProfit != null,
    rewardRiskRatio,
  };
}

function buildClosedTradeReview(
  currentTrade: ClosedTradeHistoryItem,
  history: ClosedTradeHistoryItem[],
): ClosedTradeReview {
  const reviewWindow = history
    .filter((trade) => Date.parse(trade.closedAt) <= Date.parse(currentTrade.closedAt))
    .slice(0, MAX_PATTERN_WINDOW);

  return {
    good: buildGoodRead(currentTrade),
    bad: buildBadRead(currentTrade),
    repeatedPattern: buildPatternRead(currentTrade, reviewWindow),
    provider: "Fintex",
    model: "local-review",
    wasGenerated: false,
  };
}

function buildGoodRead(trade: ClosedTradeHistoryItem) {
  if (trade.realizedProfitLoss > 0 && trade.hasStopLoss && trade.hasTakeProfit) {
    return "You closed this trade in profit with a full plan already defined on both the risk and target side.";
  }

  if (trade.realizedProfitLoss > 0) {
    return "You finished this trade green, which means the read or the exit timing worked in your favor.";
  }

  if (trade.hasStopLoss && trade.hasTakeProfit) {
    return "You traded with a complete structure in place, which is the kind of process professionals repeat.";
  }

  if (trade.hasStopLoss) {
    return "You at least defined the downside before the trade was over, which is better than trading completely open-ended.";
  }

  return "You completed the trade and added another real result to your review process, which is still useful for learning.";
}

function buildBadRead(trade: ClosedTradeHistoryItem) {
  const hasIncompletePlan = !trade.hasStopLoss || !trade.hasTakeProfit;

  if (trade.realizedProfitLoss < 0 && hasIncompletePlan) {
    return "This trade closed red and the plan was incomplete, which makes it harder to stay consistent under pressure.";
  }

  if (trade.realizedProfitLoss < 0) {
    return "This trade closed at a loss, so either the entry timing, the read, or the management did not hold up.";
  }

  if (trade.rewardRiskRatio != null && trade.rewardRiskRatio < MINIMUM_MEANINGFUL_REWARD_RISK) {
    return "The reward-to-risk profile was too thin, so the upside was not especially attractive even if the setup worked.";
  }

  if (hasIncompletePlan) {
    return "The trade did not carry a fully defined stop-loss and take-profit plan, which weakens discipline over time.";
  }

  return "There is no major structural flaw standing out in the stored trade data for this one.";
}

function buildPatternRead(
  currentTrade: ClosedTradeHistoryItem,
  reviewWindow: ClosedTradeHistoryItem[],
) {
  const losses = reviewWindow.filter((trade) => trade.realizedProfitLoss < 0);
  const wins = reviewWindow.filter((trade) => trade.realizedProfitLoss > 0);
  const sameDirectionCount = reviewWindow.filter((trade) => trade.direction === currentTrade.direction).length;
  const incompletePlans = reviewWindow.filter(
    (trade) => trade.source === "alpaca" && (!trade.hasStopLoss || !trade.hasTakeProfit),
  ).length;

  if (losses.length >= 3) {
    return "Recent losses are starting to stack up. The repeated pattern is pressing trades before the edge is clean enough.";
  }

  if (sameDirectionCount >= 3) {
    return `You keep leaning toward ${currentTrade.direction.toLowerCase()} setups. Make sure that is evidence-driven and not a standing bias.`;
  }

  if (incompletePlans >= 2) {
    return "You repeatedly trade without a fully defined plan. The pattern to break is entering before both risk and target are clear.";
  }

  if (wins.length > 0 && losses.length > 0 && averageMagnitude(losses) > averageMagnitude(wins)) {
    return "Your losses are landing larger than your winners. The repeated pattern is allowing the downside to outrun the upside.";
  }

  if (wins.length >= 3) {
    return "Your recent history shows a steadier process. Keep repeating setups where risk is defined and execution stays calm.";
  }

  return "Your recent trades are mixed. The pattern to focus on is repeating only the setups that start with clear structure and risk.";
}

function averageMagnitude(trades: ClosedTradeHistoryItem[]) {
  if (trades.length === 0) {
    return 0;
  }

  return (
    trades.reduce((total, trade) => total + Math.abs(trade.realizedProfitLoss), 0) /
    trades.length
  );
}

function getRewardRiskRatio(trade: LiveTrade) {
  if (trade.stopLoss == null || trade.takeProfit == null) {
    return null;
  }

  const reward =
    trade.direction === "Buy"
      ? trade.takeProfit - trade.entryPrice
      : trade.entryPrice - trade.takeProfit;
  const risk =
    trade.direction === "Buy"
      ? trade.entryPrice - trade.stopLoss
      : trade.stopLoss - trade.entryPrice;

  if (reward <= 0 || risk <= 0) {
    return null;
  }

  return reward / risk;
}
