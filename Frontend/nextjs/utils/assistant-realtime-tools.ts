import { getBitcoinUsdRiskInsight } from "@/utils/economic-calendar-api";
import { getExternalBrokerConnections } from "@/utils/external-broker-api";
import {
  cancelGoalTarget,
  createGoalTarget,
  getMyGoalTargets,
  pauseGoalTarget,
  resumeGoalTarget,
} from "@/utils/goal-automation-api";
import { getMyLiveTrades, placeLiveOrder, syncExternalBrokerTrades } from "@/utils/live-trading-api";
import { getRealtimeVerdict, getRelativeStrengthIndexTimeframes } from "@/utils/market-data-api";
import {
  createPriceAlert,
  deleteAlertRule,
  getMyNotificationInbox,
  markAllNotificationsAsRead,
  markNotificationAsRead,
  sendTestAlert,
} from "@/utils/notifications-api";
import {
  closePaperTradingPosition,
  createPaperTradingAccount,
  getPaperTradeRecommendation,
  getPaperTradingSnapshot,
  placePaperTradingOrder,
} from "@/utils/paper-trading-api";
import {
  createTradeAutomationRule,
  deleteTradeAutomationRule,
  getMyTradeAutomationRules,
} from "@/utils/trade-automation-api";
import { getMyUserProfile, refreshMyBehavioralProfile } from "@/utils/user-profile-api";
import {
  getMyStrategyValidationHistory,
  validateMyStrategy,
} from "@/utils/strategy-validation-api";

type JsonSchema = Record<string, unknown>;

export type RealtimeToolDefinition = {
  type: "function";
  name: string;
  description: string;
  parameters: JsonSchema;
};

export type RealtimeToolHandler = (args: Record<string, unknown>) => Promise<unknown>;

type AssistantRealtimeToolOptions = {
  onActionRefresh?: () => Promise<void> | void;
};

const DEFAULT_SELECTION = {
  key: "btcusdt-binance",
  label: "BTC / USD",
  symbol: "BTCUSDT",
  provider: 1 as const,
  venue: "Binance",
};
const DEFAULT_PAPER_SYMBOL = "BTCUSDT";
const DEFAULT_LIVE_SYMBOL = "BTCUSD";

const asString = (value: unknown, fallback = "") =>
  typeof value === "string" && value.trim().length > 0 ? value.trim() : fallback;

const asNumber = (value: unknown, fallback?: number) => {
  if (typeof value === "number" && Number.isFinite(value)) {
    return value;
  }

  if (typeof value === "string" && value.trim().length > 0) {
    const parsed = Number(value);
    if (Number.isFinite(parsed)) {
      return parsed;
    }
  }

  if (fallback != null) {
    return fallback;
  }

  throw new Error("A valid numeric value is required.");
};

const asOptionalNumber = (value: unknown) => {
  if (value == null || value === "") {
    return null;
  }

  return asNumber(value);
};

const asBoolean = (value: unknown, fallback = true) =>
  typeof value === "boolean" ? value : fallback;

const normalizePaperSymbol = (symbol: unknown) => {
  const normalized = asString(symbol, DEFAULT_PAPER_SYMBOL).toUpperCase().replace("/", "");
  return normalized === "BTC" ? DEFAULT_PAPER_SYMBOL : normalized;
};

const normalizeLiveSymbol = (symbol: unknown) => {
  const normalized = normalizePaperSymbol(symbol);
  return normalized === "BTCUSDT" ? DEFAULT_LIVE_SYMBOL : normalized;
};

const mapDirectionToCode = (value: unknown) =>
  asString(value, "Buy").toLowerCase() === "sell" ? 2 : 1;

const mapAutomationTriggerToCode = (value: unknown) => {
  switch (asString(value, "PriceTarget").toLowerCase()) {
    case "relativestrengthindex":
    case "rsi":
      return 2;
    case "macdhistogram":
      return 3;
    case "momentum":
      return 4;
    case "trendscore":
      return 5;
    case "confidencescore":
      return 6;
    case "verdict":
      return 7;
    default:
      return 1;
  }
};

const mapAutomationDestinationToCode = (value: unknown) =>
  asString(value, "PaperTrading").toLowerCase().includes("external") ? 2 : 1;

const mapVerdictToCode = (value: unknown) => {
  switch (asString(value).toLowerCase()) {
    case "buy":
      return 1;
    case "sell":
      return 2;
    case "hold":
      return 3;
    default:
      return null;
  }
};

const mapGoalAccountTypeToCode = (value: unknown) =>
  asString(value, "PaperTrading").toLowerCase().includes("external") ? 2 : 1;

const mapGoalTargetTypeToCode = (value: unknown) =>
  asString(value, "PercentGrowth").toLowerCase().includes("amount") ? 2 : 1;

const mapGoalTradingSessionToCode = (value: unknown) => {
  switch (asString(value, "AnyTime").toLowerCase()) {
    case "europe":
      return 2;
    case "us":
      return 3;
    case "europeusoverlap":
    case "overlap":
      return 4;
    default:
      return 1;
  }
};

const resolveDefaultConnectionId = async () => {
  const connections = await getExternalBrokerConnections();
  const connected = connections.filter((item) => item.isActive && item.status === "Connected");
  return connected.length === 1 ? connected[0]?.id ?? null : null;
};

const refreshAfterMutation = async (callback?: AssistantRealtimeToolOptions["onActionRefresh"]) => {
  try {
    await callback?.();
  } catch {
    // Keep tool execution successful even if the surrounding dashboard refresh lags behind.
  }
};

const settle = async <T>(promise: Promise<T>) => {
  try {
    return await promise;
  } catch (error) {
    return {
      error: error instanceof Error ? error.message : "Request failed.",
    };
  }
};

export const assistantRealtimeTools: RealtimeToolDefinition[] = [
  {
    type: "function",
    name: "get_dashboard_context",
    description: "Load the freshest Fintex dashboard context including verdict, paper account, alerts, goals, automation, live trades, behavior, broker connections, macro risk, and strategy history.",
    parameters: { type: "object", properties: {}, additionalProperties: false },
  },
  {
    type: "function",
    name: "get_trade_recommendation",
    description: "Get the latest BTC paper-trading recommendation, including reasons, risk, stop loss, and take profit guidance.",
    parameters: {
      type: "object",
      properties: {
        quantity: { type: "number" },
        stopLoss: { type: "number" },
        takeProfit: { type: "number" },
        symbol: { type: "string" },
      },
      additionalProperties: false,
    },
  },
  {
    type: "function",
    name: "create_paper_account",
    description: "Create the in-app paper trading account for the current user.",
    parameters: {
      type: "object",
      properties: {
        name: { type: "string" },
        baseCurrency: { type: "string" },
        startingBalance: { type: "number" },
      },
      required: ["name", "startingBalance"],
      additionalProperties: false,
    },
  },
  {
    type: "function",
    name: "place_paper_trade",
    description: "Place an in-app paper market order with optional stop loss and take profit.",
    parameters: {
      type: "object",
      properties: {
        direction: { type: "string", enum: ["Buy", "Sell"] },
        quantity: { type: "number" },
        symbol: { type: "string" },
        stopLoss: { type: "number" },
        takeProfit: { type: "number" },
        notes: { type: "string" },
      },
      required: ["direction", "quantity"],
      additionalProperties: false,
    },
  },
  {
    type: "function",
    name: "close_paper_position",
    description: "Close an open paper position immediately.",
    parameters: {
      type: "object",
      properties: {
        positionId: { type: "number" },
        quantity: { type: "number" },
        exitPrice: { type: "number" },
      },
      required: ["positionId"],
      additionalProperties: false,
    },
  },
  {
    type: "function",
    name: "place_live_trade",
    description: "Place a live broker market order on a connected broker account.",
    parameters: {
      type: "object",
      properties: {
        connectionId: { type: "number" },
        direction: { type: "string", enum: ["Buy", "Sell"] },
        quantity: { type: "number" },
        symbol: { type: "string" },
        stopLoss: { type: "number" },
        takeProfit: { type: "number" },
        notes: { type: "string" },
      },
      required: ["direction", "quantity"],
      additionalProperties: false,
    },
  },
  {
    type: "function",
    name: "sync_live_trades",
    description: "Sync live trades and broker state from connected external broker accounts.",
    parameters: { type: "object", properties: {}, additionalProperties: false },
  },
  {
    type: "function",
    name: "create_price_alert",
    description: "Create a user-owned BTC price alert with in-app and/or email delivery.",
    parameters: {
      type: "object",
      properties: {
        targetPrice: { type: "number" },
        name: { type: "string" },
        symbol: { type: "string" },
        notifyInApp: { type: "boolean" },
        notifyEmail: { type: "boolean" },
        notes: { type: "string" },
      },
      required: ["targetPrice"],
      additionalProperties: false,
    },
  },
  {
    type: "function",
    name: "delete_alert_rule",
    description: "Delete an existing user-created alert rule.",
    parameters: {
      type: "object",
      properties: { ruleId: { type: "number" } },
      required: ["ruleId"],
      additionalProperties: false,
    },
  },
  {
    type: "function",
    name: "mark_notification_read",
    description: "Mark one in-app notification as read.",
    parameters: {
      type: "object",
      properties: { notificationId: { type: "number" } },
      required: ["notificationId"],
      additionalProperties: false,
    },
  },
  {
    type: "function",
    name: "mark_all_notifications_read",
    description: "Mark all unread in-app notifications as read.",
    parameters: { type: "object", properties: {}, additionalProperties: false },
  },
  {
    type: "function",
    name: "send_test_alert",
    description: "Send a test in-app and email notification using the alert delivery path.",
    parameters: { type: "object", properties: {}, additionalProperties: false },
  },
  {
    type: "function",
    name: "create_goal_target",
    description: "Create a BTC goal automation target for paper trading or an external broker connection.",
    parameters: {
      type: "object",
      properties: {
        accountType: { type: "string", enum: ["PaperTrading", "ExternalBroker"] },
        externalConnectionId: { type: "number" },
        targetType: { type: "string", enum: ["PercentGrowth", "TargetAmount"] },
        targetPercent: { type: "number" },
        targetAmount: { type: "number" },
        deadlineUtc: { type: "string" },
        name: { type: "string" },
        maxAcceptableRisk: { type: "number" },
        maxDrawdownPercent: { type: "number" },
        maxPositionSizePercent: { type: "number" },
        tradingSession: { type: "string", enum: ["AnyTime", "Europe", "Us", "EuropeUsOverlap"] },
        allowOvernightPositions: { type: "boolean" },
      },
      required: ["accountType", "targetType", "deadlineUtc"],
      additionalProperties: false,
    },
  },
  {
    type: "function",
    name: "pause_goal_target",
    description: "Pause a goal automation target.",
    parameters: { type: "object", properties: { goalId: { type: "number" } }, required: ["goalId"], additionalProperties: false },
  },
  {
    type: "function",
    name: "resume_goal_target",
    description: "Resume a paused goal automation target.",
    parameters: { type: "object", properties: { goalId: { type: "number" } }, required: ["goalId"], additionalProperties: false },
  },
  {
    type: "function",
    name: "cancel_goal_target",
    description: "Cancel a goal automation target.",
    parameters: { type: "object", properties: { goalId: { type: "number" } }, required: ["goalId"], additionalProperties: false },
  },
  {
    type: "function",
    name: "create_trade_automation_rule",
    description: "Create an automatic trade execution rule for the current user.",
    parameters: {
      type: "object",
      properties: {
        name: { type: "string" },
        triggerType: { type: "string" },
        tradeDirection: { type: "string", enum: ["Buy", "Sell"] },
        quantity: { type: "number" },
        symbol: { type: "string" },
        triggerValue: { type: "number" },
        targetVerdict: { type: "string", enum: ["Buy", "Sell", "Hold"] },
        minimumConfidenceScore: { type: "number" },
        destination: { type: "string", enum: ["PaperTrading", "ExternalBroker"] },
        externalConnectionId: { type: "number" },
        stopLoss: { type: "number" },
        takeProfit: { type: "number" },
        notifyInApp: { type: "boolean" },
        notifyEmail: { type: "boolean" },
        notes: { type: "string" },
      },
      required: ["name", "triggerType", "tradeDirection", "quantity"],
      additionalProperties: false,
    },
  },
  {
    type: "function",
    name: "delete_trade_automation_rule",
    description: "Delete one of the current user's automation rules.",
    parameters: { type: "object", properties: { ruleId: { type: "number" } }, required: ["ruleId"], additionalProperties: false },
  },
  {
    type: "function",
    name: "validate_strategy",
    description: "Run AI strategy validation against the current BTC market context.",
    parameters: {
      type: "object",
      properties: {
        strategyText: { type: "string" },
        strategyName: { type: "string" },
        timeframe: { type: "string" },
        directionPreference: { type: "string" },
        symbol: { type: "string" },
      },
      required: ["strategyText"],
      additionalProperties: false,
    },
  },
  {
    type: "function",
    name: "refresh_behavior_analysis",
    description: "Refresh the current user's behavioral analysis profile.",
    parameters: { type: "object", properties: {}, additionalProperties: false },
  },
];

export const createAssistantRealtimeToolHandlers = ({
  onActionRefresh,
}: AssistantRealtimeToolOptions): Record<string, RealtimeToolHandler> => ({
  get_dashboard_context: async () => {
    const [verdict, timeframeRsi, paperSnapshot, recommendation, notifications, behaviorProfile, liveTrades, brokerConnections, goalTargets, tradeAutomationRules, macroInsight, strategyValidationHistory] = await Promise.all([
      settle(getRealtimeVerdict(DEFAULT_SELECTION)),
      settle(getRelativeStrengthIndexTimeframes(DEFAULT_SELECTION)),
      settle(getPaperTradingSnapshot()),
      settle(getPaperTradeRecommendation({ symbol: DEFAULT_PAPER_SYMBOL, assetClass: 1, provider: 1 })),
      settle(getMyNotificationInbox()),
      settle(getMyUserProfile()),
      settle(getMyLiveTrades()),
      settle(getExternalBrokerConnections()),
      settle(getMyGoalTargets()),
      settle(getMyTradeAutomationRules()),
      settle(getBitcoinUsdRiskInsight()),
      settle(getMyStrategyValidationHistory()),
    ]);

    return { verdict, timeframeRsi, paperSnapshot, recommendation, notifications, behaviorProfile, liveTrades, brokerConnections, goalTargets, tradeAutomationRules, macroInsight, strategyValidationHistory };
  },
  get_trade_recommendation: async (args) => getPaperTradeRecommendation({
    symbol: normalizePaperSymbol(args.symbol),
    assetClass: 1,
    provider: 1,
    quantity: asOptionalNumber(args.quantity),
    stopLoss: asOptionalNumber(args.stopLoss),
    takeProfit: asOptionalNumber(args.takeProfit),
  }),
  create_paper_account: async (args) => {
    await createPaperTradingAccount({ name: asString(args.name, "Fintex Paper"), baseCurrency: asString(args.baseCurrency, "USD"), startingBalance: asNumber(args.startingBalance) });
    await refreshAfterMutation(onActionRefresh);
    return { status: "completed", summary: "Paper trading account created." };
  },
  place_paper_trade: async (args) => {
    const execution = await placePaperTradingOrder({
      symbol: normalizePaperSymbol(args.symbol),
      assetClass: 1,
      provider: 1,
      direction: mapDirectionToCode(args.direction) === 2 ? "Sell" : "Buy",
      quantity: asNumber(args.quantity),
      stopLoss: asOptionalNumber(args.stopLoss),
      takeProfit: asOptionalNumber(args.takeProfit),
      notes: asString(args.notes),
    });
    await refreshAfterMutation(onActionRefresh);
    return execution;
  },
  close_paper_position: async (args) => {
    const order = await closePaperTradingPosition({ positionId: asNumber(args.positionId), quantity: asOptionalNumber(args.quantity), exitPrice: asOptionalNumber(args.exitPrice) });
    await refreshAfterMutation(onActionRefresh);
    return order;
  },
  place_live_trade: async (args) => {
    const connectionId = asOptionalNumber(args.connectionId) ?? await resolveDefaultConnectionId();
    if (connectionId == null) {
      throw new Error("A connected broker account is required before placing a live trade.");
    }

    const execution = await placeLiveOrder({
      connectionId,
      symbol: normalizeLiveSymbol(args.symbol),
      assetClass: 1,
      provider: 1,
      direction: mapDirectionToCode(args.direction) === 2 ? "Sell" : "Buy",
      quantity: asNumber(args.quantity),
      stopLoss: asOptionalNumber(args.stopLoss),
      takeProfit: asOptionalNumber(args.takeProfit),
      notes: asString(args.notes),
    });
    await refreshAfterMutation(onActionRefresh);
    return execution;
  },
  sync_live_trades: async () => {
    await syncExternalBrokerTrades();
    await refreshAfterMutation(onActionRefresh);
    return { status: "completed", summary: "Live trades synchronized." };
  },
  create_price_alert: async (args) => {
    await createPriceAlert({
      name: asString(args.name, "Assistant BTC alert"),
      symbol: normalizePaperSymbol(args.symbol),
      provider: 1,
      targetPrice: asNumber(args.targetPrice),
      notifyInApp: asBoolean(args.notifyInApp, true),
      notifyEmail: asBoolean(args.notifyEmail, true),
      notes: asString(args.notes),
    });
    await refreshAfterMutation(onActionRefresh);
    return { status: "completed", summary: "Price alert created." };
  },
  delete_alert_rule: async (args) => {
    await deleteAlertRule(asNumber(args.ruleId));
    await refreshAfterMutation(onActionRefresh);
    return { status: "completed", summary: "Alert rule deleted." };
  },
  mark_notification_read: async (args) => {
    await markNotificationAsRead(asNumber(args.notificationId));
    await refreshAfterMutation(onActionRefresh);
    return { status: "completed", summary: "Notification marked as read." };
  },
  mark_all_notifications_read: async () => {
    await markAllNotificationsAsRead();
    await refreshAfterMutation(onActionRefresh);
    return { status: "completed", summary: "All notifications marked as read." };
  },
  send_test_alert: async () => {
    await sendTestAlert();
    await refreshAfterMutation(onActionRefresh);
    return { status: "completed", summary: "Test alert sent." };
  },
  create_goal_target: async (args) => {
    const wantsExternal = mapGoalAccountTypeToCode(args.accountType) === 2;
    const externalConnectionId = wantsExternal ? asOptionalNumber(args.externalConnectionId) ?? await resolveDefaultConnectionId() : null;
    await createGoalTarget({
      name: asString(args.name),
      accountType: mapGoalAccountTypeToCode(args.accountType),
      externalConnectionId,
      targetType: mapGoalTargetTypeToCode(args.targetType),
      targetPercent: asOptionalNumber(args.targetPercent),
      targetAmount: asOptionalNumber(args.targetAmount),
      deadlineUtc: asString(args.deadlineUtc),
      maxAcceptableRisk: asNumber(args.maxAcceptableRisk, 45),
      maxDrawdownPercent: asNumber(args.maxDrawdownPercent, 2.5),
      maxPositionSizePercent: asNumber(args.maxPositionSizePercent, 20),
      tradingSession: mapGoalTradingSessionToCode(args.tradingSession),
      allowOvernightPositions: asBoolean(args.allowOvernightPositions, true),
    });
    await refreshAfterMutation(onActionRefresh);
    return { status: "completed", summary: "Goal target created." };
  },
  pause_goal_target: async (args) => {
    await pauseGoalTarget(asNumber(args.goalId));
    await refreshAfterMutation(onActionRefresh);
    return { status: "completed", summary: "Goal paused." };
  },
  resume_goal_target: async (args) => {
    await resumeGoalTarget(asNumber(args.goalId));
    await refreshAfterMutation(onActionRefresh);
    return { status: "completed", summary: "Goal resumed." };
  },
  cancel_goal_target: async (args) => {
    await cancelGoalTarget(asNumber(args.goalId));
    await refreshAfterMutation(onActionRefresh);
    return { status: "completed", summary: "Goal canceled." };
  },
  create_trade_automation_rule: async (args) => {
    const external = mapAutomationDestinationToCode(args.destination) === 2;
    const externalConnectionId = external ? asOptionalNumber(args.externalConnectionId) ?? await resolveDefaultConnectionId() : null;
    await createTradeAutomationRule({
      name: asString(args.name),
      symbol: normalizePaperSymbol(args.symbol),
      provider: 1,
      triggerType: mapAutomationTriggerToCode(args.triggerType),
      triggerValue: asOptionalNumber(args.triggerValue),
      targetVerdict: mapVerdictToCode(args.targetVerdict),
      minimumConfidenceScore: asOptionalNumber(args.minimumConfidenceScore),
      destination: mapAutomationDestinationToCode(args.destination),
      externalConnectionId,
      tradeDirection: mapDirectionToCode(args.tradeDirection),
      quantity: asNumber(args.quantity),
      stopLoss: asOptionalNumber(args.stopLoss),
      takeProfit: asOptionalNumber(args.takeProfit),
      notifyInApp: asBoolean(args.notifyInApp, true),
      notifyEmail: asBoolean(args.notifyEmail, true),
      notes: asString(args.notes),
    });
    await refreshAfterMutation(onActionRefresh);
    return { status: "completed", summary: "Trade automation rule created." };
  },
  delete_trade_automation_rule: async (args) => {
    await deleteTradeAutomationRule(asNumber(args.ruleId));
    await refreshAfterMutation(onActionRefresh);
    return { status: "completed", summary: "Trade automation rule deleted." };
  },
  validate_strategy: async (args) => validateMyStrategy({
    strategyName: asString(args.strategyName),
    symbol: normalizePaperSymbol(args.symbol),
    provider: 1,
    timeframe: asString(args.timeframe, "1m"),
    directionPreference: asString(args.directionPreference),
    strategyText: asString(args.strategyText),
  }),
  refresh_behavior_analysis: async () => {
    const profile = await refreshMyBehavioralProfile();
    await refreshAfterMutation(onActionRefresh);
    return profile;
  },
});
