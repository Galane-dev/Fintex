from xml.etree.ElementTree import Element, SubElement, ElementTree


PAGE_WIDTH = "4200"
PAGE_HEIGHT = "2800"

CONTAINER_STYLE = (
    "rounded=1;whiteSpace=wrap;html=1;fontSize=16;fontStyle=1;"
    "align=left;verticalAlign=top;spacing=12;"
)
ENTITY_STYLE = (
    "rounded=1;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#111827;"
    "align=left;verticalAlign=top;spacing=8;"
)
VALUE_STYLE = ENTITY_STYLE + "dashed=1;"
NOTE_STYLE = (
    "shape=note;whiteSpace=wrap;html=1;fillColor=#fff7d6;strokeColor=#b45309;"
    "align=left;verticalAlign=top;spacing=10;fontSize=13;"
)
COMP_STYLE = (
    "edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;"
    "startArrow=diamond;startFill=1;endArrow=none;strokeWidth=1.4;"
)
AGGR_STYLE = (
    "edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;"
    "startArrow=diamond;startFill=0;endArrow=none;strokeWidth=1.4;"
)
DEP_STYLE = (
    "edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;"
    "dashed=1;endArrow=open;strokeWidth=1.2;"
)
ASSOC_STYLE = (
    "edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;"
    "endArrow=open;strokeWidth=1.2;"
)
OWN_STYLE = (
    "edgeStyle=orthogonalEdgeStyle;rounded=0;orthogonalLoop=1;jettySize=auto;html=1;"
    "endArrow=block;endFill=1;strokeWidth=1.3;"
)


def v(root, cell_id, value, x, y, w, h, style):
    cell = SubElement(
        root,
        "mxCell",
        {
            "id": str(cell_id),
            "value": value,
            "style": style,
            "vertex": "1",
            "parent": "1",
        },
    )
    SubElement(
        cell,
        "mxGeometry",
        {
            "x": str(x),
            "y": str(y),
            "width": str(w),
            "height": str(h),
            "as": "geometry",
        },
    )


def e(root, cell_id, value, source, target, style):
    cell = SubElement(
        root,
        "mxCell",
        {
            "id": str(cell_id),
            "value": value,
            "style": style,
            "edge": "1",
            "parent": "1",
            "source": str(source),
            "target": str(target),
        },
    )
    SubElement(cell, "mxGeometry", {"relative": "1", "as": "geometry"})


def entity(title, kind, *lines):
    body = "<br>".join(lines)
    return f"&lt;b&gt;{title}&lt;/b&gt;&lt;br&gt;&lt;font color=&quot;#666666&quot;&gt;&lt;i&gt;{kind}&lt;/i&gt;&lt;/font&gt;&lt;br&gt;{body}"


mxfile = Element("mxfile", {"host": "app.diagrams.net", "agent": "Codex", "version": "24.7.17"})
diagram = SubElement(mxfile, "diagram", {"id": "fintex-domain-model", "name": "Fintex Domain Model"})
graph = SubElement(
    diagram,
    "mxGraphModel",
    {
        "dx": "1794",
        "dy": "1010",
        "grid": "1",
        "gridSize": "10",
        "guides": "1",
        "tooltips": "1",
        "connect": "1",
        "arrows": "1",
        "fold": "1",
        "page": "1",
        "pageScale": "1",
        "pageWidth": PAGE_WIDTH,
        "pageHeight": PAGE_HEIGHT,
        "math": "0",
        "shadow": "0",
    },
)
root = SubElement(graph, "root")
SubElement(root, "mxCell", {"id": "0"})
SubElement(root, "mxCell", {"id": "1", "parent": "0"})

v(
    root,
    2,
    "&lt;b&gt;Notation&lt;/b&gt;&lt;br&gt;Filled diamond = composition / part of&lt;br&gt;"
    "Hollow diamond = aggregation&lt;br&gt;Dashed arrow = uses, derives, summarizes, or may create&lt;br&gt;"
    "Technical IDs are intentionally hidden.&lt;br&gt;&lt;br&gt;&lt;b&gt;Enum format&lt;/b&gt;&lt;br&gt;"
    "MarketVerdict: RefList(MarketVerdict)&lt;br&gt;(Hold, Buy, Sell)",
    20,
    20,
    450,
    180,
    NOTE_STYLE,
)

containers = [
    (10, "Platform Foundation", 500, 20, 620, 240, "#f3f4f6", "#6b7280"),
    (20, "Market Data Context", 1170, 20, 760, 430, "#dbeafe", "#1d4ed8"),
    (30, "News Context", 1980, 20, 860, 430, "#ffedd5", "#c2410c"),
    (40, "Trading Context", 20, 320, 760, 540, "#dcfce7", "#15803d"),
    (50, "External Broker Context", 830, 500, 700, 430, "#ede9fe", "#6d28d9"),
    (60, "Paper Trading Context", 1580, 480, 1220, 600, "#ccfbf1", "#0f766e"),
    (70, "Notifications Context", 20, 900, 780, 520, "#fee2e2", "#b91c1c"),
    (80, "Automation Context", 850, 960, 560, 340, "#ecfccb", "#4d7c0f"),
    (90, "Goals Context", 1440, 1120, 1360, 660, "#f5d0fe", "#a21caf"),
    (100, "Profiles, Academy, Strategy Context", 20, 1480, 1360, 720, "#fce7f3", "#be185d"),
]

for cell_id, title, x, y, w, h, fill, stroke in containers:
    v(root, cell_id, title, x, y, w, h, CONTAINER_STYLE + f"fillColor={fill};strokeColor={stroke};")

vertices = [
    (11, entity("Tenant", "Platform root", "TenancyName: string", "Name: string"), 530, 70, 170, 110, ENTITY_STYLE),
    (12, entity("User", "Platform root", "UserName: string", "Name: string", "Surname: string", "EmailAddress: string"), 740, 55, 180, 140, ENTITY_STYLE),
    (13, entity("Role", "Platform root", "Name: string", "DisplayName: string", "Description: string"), 960, 70, 130, 110, ENTITY_STYLE),

    (21, entity("MarketDataPoint", "Aggregate", "Symbol: string", "AssetClass: RefList(AssetClass)", "Provider: RefList(MarketDataProvider)", "Price / Bid / Ask: decimal", "Volume: decimal?", "Timestamp: DateTime", "Sma / Ema / Rsi / StdDev", "Macd / MacdSignal / MacdHistogram", "Momentum / RateOfChange", "BollingerUpper / BollingerLower", "TrendScore / ConfidenceScore", "Verdict: RefList(MarketVerdict)", "(Hold, Buy, Sell)"), 1200, 70, 270, 320, ENTITY_STYLE),
    (22, entity("MarketDataTimeframeCandle", "Aggregate", "Symbol: string", "AssetClass: RefList(AssetClass)", "Provider: RefList(MarketDataProvider)", "Timeframe: RefList(MarketDataTimeframe)", "OpenTime: DateTime", "Open / High / Low / Close: decimal", "LastPriceTimestamp: DateTime"), 1520, 90, 190, 220, ENTITY_STYLE),
    (23, entity("IndicatorScore", "Value object", "Name: string", "Value: decimal", "Score: decimal", "Signal: RefList(IndicatorSignal)"), 1520, 330, 190, 90, VALUE_STYLE),
    (24, entity("Domain Events", "Event contracts", "MarketDataUpdatedEventData", "TradeExecutedEventData", "TradeAnalysisCompletedEventData", "NotificationCreatedEventData"), 1750, 110, 150, 160, VALUE_STYLE),

    (31, entity("NewsSource", "Aggregate", "Name: string", "SourceKind: RefList(NewsSourceKind)", "SiteUrl: string", "FeedUrl: string", "Category: string", "FocusTags: string", "IsActive: bool", "Refresh status fields"), 2010, 70, 190, 220, ENTITY_STYLE),
    (32, entity("NewsArticle", "Aggregate", "Url: string", "Title: string", "Summary: string", "PublishedAt: DateTime?", "Author: string", "Category: string", "Tags: string", "ContentHash: string", "IsBitcoinRelevant: bool", "IsUsdRelevant: bool", "RelevanceScore: int", "LastSeenAt: DateTime"), 2250, 70, 240, 290, ENTITY_STYLE),
    (33, entity("NewsRefreshRun", "Aggregate", "FocusKey: string", "Trigger: string", "StartedAt / CompletedAt", "Status: RefList(NewsRefreshStatus)", "(Started, Completed, Skipped, Failed)", "SourceCount / ArticlesFetched / NewArticles", "Summary: string"), 2540, 70, 230, 220, ENTITY_STYLE),
    (34, entity("NewsAnalysisSnapshot", "Aggregate", "FocusKey: string", "GeneratedAt: DateTime", "ArticleCount: int", "Sentiment: RefList(NewsImpactSentiment)", "(Bearish, Neutral, Bullish)", "ImpactScore: decimal", "RecommendedAction: RefList(MarketVerdict)", "(Hold, Buy, Sell)", "Summary: string", "KeyHeadlines: string", "Provider / Model: string"), 2540, 320, 230, 110, ENTITY_STYLE),

    (41, entity("Trade", "Aggregate", "Symbol: string", "AssetClass: RefList(AssetClass)", "Provider: RefList(MarketDataProvider)", "Direction: RefList(TradeDirection)", "(Buy, Sell)", "Status: RefList(TradeStatus)", "(Open, Closed, Cancelled)", "Quantity: decimal", "EntryPrice / ExitPrice: decimal", "StopLoss / TakeProfit: decimal?", "RealizedProfitLoss / UnrealizedProfitLoss", "LastMarketPrice: decimal", "CurrentRiskScore: decimal", "CurrentRecommendation: string", "ExecutedAt / ClosedAt: DateTime"), 50, 370, 250, 400, ENTITY_STYLE),
    (42, entity("TradeAnalysisSnapshot", "Aggregate", "GeneratedAt: DateTime", "SmaValue / EmaValue", "RsiValue / StdDevValue", "CompositeRiskScore: decimal", "Recommendation: RefList(TradeRecommendation)", "(Buy, Sell, Hold, Monitor)", "Narrative: string", "BehavioralSummary: string", "ExternalAiProvider / Model"), 350, 390, 190, 250, ENTITY_STYLE),
    (43, entity("TradeExecutionContext", "Aggregate", "BrokerProvider / Platform", "BrokerEnvironment / BrokerSymbol", "Direction: RefList(TradeDirection)", "AssetClass: RefList(AssetClass)", "MarketDataProvider: RefList(MarketDataProvider)", "Quantity / ReferencePrice", "Bid / Ask / Spread / SpreadPercent", "StopLoss / TakeProfit", "MarketVerdict: RefList(MarketVerdict)", "(Hold, Buy, Sell)", "TrendScore / ConfidenceScore", "StructureScore / StructureLabel", "Core indicators snapshot", "User risk fields", "Broker submission and fill details"), 570, 350, 180, 430, ENTITY_STYLE),

    (51, entity("ExternalBrokerConnection", "Aggregate", "DisplayName: string", "Provider: RefList(ExternalBrokerProvider)", "Platform: RefList(ExternalBrokerPlatform)", "AccountLogin: string", "Server: string", "EncryptedPassword: string", "TerminalPath: string", "Status: RefList(ExternalBrokerConnectionStatus)", "(Pending, Connected, Failed, Disconnected)", "IsActive: bool", "LastValidatedAt / LastSyncedAt", "LastError: string", "BrokerAccountName / Currency / Company", "BrokerLeverage", "LastKnownBalance / Equity"), 860, 550, 260, 330, ENTITY_STYLE),
    (52, entity("ExternalBrokerExecutionEvent", "Aggregate", "BrokerProvider / Platform", "BrokerEnvironment: string", "EventType: string", "ExecutionId: string", "BrokerOrderId / ClientOrderId", "BrokerSymbol / NormalizedSymbol", "BrokerOrderStatus: string", "Direction: RefList(TradeDirection)", "(Buy, Sell)", "AssetClass: RefList(AssetClass)", "Quantity / FilledQuantity / EventQuantity", "Price / FilledAveragePrice", "PositionQuantity", "OccurredAt: DateTime", "RawPayloadJson"), 1180, 550, 320, 300, ENTITY_STYLE),

    (61, entity("PaperTradingAccount", "Aggregate", "Name: string", "BaseCurrency: string", "StartingBalance: decimal", "CashBalance: decimal", "Equity: decimal", "RealizedProfitLoss / UnrealizedProfitLoss", "IsActive: bool", "LastMarkedToMarketAt: DateTime"), 1610, 560, 220, 220, ENTITY_STYLE),
    (62, entity("PaperOrder", "Aggregate", "Symbol: string", "AssetClass: RefList(AssetClass)", "Provider: RefList(MarketDataProvider)", "Direction: RefList(TradeDirection)", "(Buy, Sell)", "OrderType: RefList(PaperOrderType)", "Status: RefList(PaperOrderStatus)", "Quantity: decimal", "RequestedPrice / ExecutedPrice", "StopLoss / TakeProfit", "Notes: string", "SubmittedAt / ExecutedAt: DateTime"), 1880, 540, 260, 300, ENTITY_STYLE),
    (63, entity("PaperPosition", "Aggregate", "Symbol: string", "AssetClass: RefList(AssetClass)", "Provider: RefList(MarketDataProvider)", "Direction: RefList(TradeDirection)", "(Buy, Sell)", "Status: RefList(PaperPositionStatus)", "(Open, Closed)", "Quantity: decimal", "AverageEntryPrice / CurrentMarketPrice", "RealizedProfitLoss / UnrealizedProfitLoss", "StopLoss / TakeProfit", "OpenedAt / LastUpdatedAt / ClosedAt"), 2190, 540, 280, 320, ENTITY_STYLE),
    (64, entity("PaperTradeFill", "Aggregate", "Symbol: string", "AssetClass: RefList(AssetClass)", "Provider: RefList(MarketDataProvider)", "Direction: RefList(TradeDirection)", "(Buy, Sell)", "Quantity: decimal", "Price: decimal", "RealizedProfitLoss: decimal", "ExecutedAt: DateTime"), 2510, 590, 250, 200, ENTITY_STYLE),

    (71, entity("NotificationAlertRule", "Aggregate", "Name: string", "Symbol: string", "Provider: RefList(MarketDataProvider)", "AlertType: RefList(NotificationAlertRuleType)", "Direction: RefList(NotificationAlertDirection)", "(Above, Below)", "CreatedPrice / LastObservedPrice", "TargetPrice: decimal", "NotifyInApp / NotifyEmail", "IsActive: bool", "LastTriggeredAt: DateTime?", "Notes: string"), 50, 980, 240, 310, ENTITY_STYLE),
    (72, entity("NotificationItem", "Aggregate", "Type: RefList(NotificationType)", "Severity: RefList(NotificationSeverity)", "Title / Message", "Symbol: string", "Provider: RefList(MarketDataProvider)", "ReferencePrice / TargetPrice", "ConfidenceScore: decimal?", "Verdict: RefList(MarketVerdict)", "(Hold, Buy, Sell)", "TriggerKey: string", "Read, email, and in-app delivery states", "OccurredAt: DateTime", "ContextJson: string"), 330, 970, 250, 340, ENTITY_STYLE),
    (73, entity("NotificationMarketSnapshot", "Value object", "Symbol: string", "Provider: RefList(MarketDataProvider)", "Price / Bid / Ask", "Rsi / MacdHistogram / Momentum", "Verdict: RefList(MarketVerdict)", "(Hold, Buy, Sell)", "ConfidenceScore / TrendScore"), 620, 1010, 150, 190, VALUE_STYLE),

    (81, entity("TradeAutomationRule", "Aggregate", "Name: string", "Symbol: string", "Provider: RefList(MarketDataProvider)", "TriggerType: RefList(TradeAutomationTriggerType)", "CreatedMetricValue / LastObservedMetricValue", "TargetMetricValue: decimal?", "TargetVerdict: RefList(MarketVerdict)", "(Hold, Buy, Sell)", "MinimumConfidenceScore: decimal?", "Destination: RefList(TradeAutomationDestination)", "(PaperAccount, ExternalBroker)", "TradeDirection: RefList(TradeDirection)", "(Buy, Sell)", "Quantity: decimal", "StopLoss / TakeProfit", "NotifyInApp / NotifyEmail", "IsActive / LastTriggeredAt", "Notes: string"), 880, 1030, 460, 270, ENTITY_STYLE),

    (91, entity("GoalTarget", "Aggregate", "Name: string", "AccountType: RefList(GoalAccountType)", "(PaperTrading, ExternalBroker)", "MarketSymbol / AllowedSymbols", "TargetType: RefList(GoalTargetType)", "(GrowthPercent, TargetEquity)", "StartEquity / CurrentEquity / TargetEquity", "TargetPercent: decimal", "DeadlineUtc: DateTime", "MaxAcceptableRisk / MaxDrawdownPercent", "MaxPositionSizePercent", "TradingSession: RefList(GoalTradingSession)", "AllowOvernightPositions: bool", "Status: RefList(GoalStatus)", "(Draft, Accepted, Rejected, Active, Paused, Completed, Expired, Canceled)", "StatusReason / LatestPlanSummary / LatestNextAction", "ProgressPercent / RequiredDailyGrowthPercent", "Execution timestamps and counters", "LastError: string"), 1450, 1170, 300, 500, ENTITY_STYLE),
    (92, entity("GoalEvaluationRun", "Aggregate", "GoalStatus: RefList(GoalStatus)", "CurrentEquity: decimal", "RequiredGrowthPercent", "RequiredDailyGrowthPercent", "FeasibilityScore: decimal", "Summary: string", "CounterProposalTargetEquity", "CounterProposalTargetPercent", "OccurredAtUtc: DateTime"), 1810, 1180, 240, 220, ENTITY_STYLE),
    (93, entity("GoalExecutionPlan", "Aggregate", "ExecutionSymbol: string", "SuggestedDirection: RefList(TradeDirection)", "(Buy, Sell)", "SuggestedQuantity: decimal?", "SuggestedStopLoss / SuggestedTakeProfit", "RiskScore: decimal?", "Summary: string", "NextAction: string", "GeneratedAtUtc: DateTime"), 2100, 1180, 240, 220, ENTITY_STYLE),
    (94, entity("GoalExecutionEvent", "Aggregate", "EventType: string", "Status: string", "Summary: string", "EquityAfterExecution: decimal?", "OccurredAtUtc: DateTime"), 2390, 1180, 210, 180, ENTITY_STYLE),

    (101, entity("UserProfile", "Aggregate", "PreferredBaseCurrency: string", "FavoriteSymbols: string", "RiskTolerance: decimal", "IsAiInsightsEnabled: bool", "BehavioralRiskScore: decimal", "BehavioralSummary: string", "StrategyNotes: string", "LastAiProvider / LastAiModel", "LastBehavioralAnalysisTime: DateTime", "AcademyStage: RefList(AcademyStage)", "(IntroCourse, TradeAcademy, Graduated)", "IntroQuizAttemptsCount / BestIntroQuizScore", "IntroQuizPassedAt / AcademyGraduatedAt", "CurrentIntroLessonKey", "CompletedIntroLessonKeys"), 50, 1540, 280, 420, ENTITY_STYLE),
    (102, entity("AcademyQuizAttempt", "Aggregate", "CourseKey: string", "CorrectAnswers / TotalQuestions", "ScorePercent: decimal", "RequiredScorePercent: decimal", "Passed: bool", "AnswersJson: string"), 380, 1560, 220, 180, ENTITY_STYLE),
    (103, entity("StrategyValidationRun", "Aggregate", "StrategyName: string", "Symbol: string", "Provider: RefList(MarketDataProvider)", "Timeframe: string", "DirectionPreference: string", "StrategyText: string", "MarketPrice / MarketTrendScore / MarketConfidenceScore", "MarketVerdict: RefList(MarketVerdict)", "(Hold, Buy, Sell)", "NewsSummary: string", "ValidationScore: decimal", "Outcome: RefList(StrategyValidationOutcome)", "(Invalid, Weak, Mixed, Valid, Strong)", "Summary: string", "StrengthsJson / RisksJson / ImprovementsJson", "SuggestedAction / Entry / StopLoss / TakeProfit", "AiProvider / AiModel"), 650, 1540, 700, 360, ENTITY_STYLE),
]

for item in vertices:
    v(root, *item)

edges = [
    (200, "1..*", 11, 12, OWN_STYLE),
    (201, "1..*", 11, 13, OWN_STYLE),
    (210, "part of", 12, 101, COMP_STYLE),
    (211, "owned by", 12, 41, AGGR_STYLE),
    (212, "owned by", 12, 51, AGGR_STYLE),
    (213, "owned by", 12, 61, AGGR_STYLE),
    (214, "owned by", 12, 71, AGGR_STYLE),
    (215, "owned by", 12, 72, AGGR_STYLE),
    (216, "owned by", 12, 81, AGGR_STYLE),
    (217, "owned by", 12, 91, AGGR_STYLE),
    (218, "owned by", 12, 102, AGGR_STYLE),
    (219, "owned by", 12, 103, AGGR_STYLE),
    (220, "part of", 41, 42, COMP_STYLE),
    (221, "part of", 41, 43, COMP_STYLE),
    (222, "uses", 43, 21, DEP_STYLE),
    (223, "uses", 43, 101, DEP_STYLE),
    (224, "publishes", 31, 32, COMP_STYLE),
    (225, "summarizes", 34, 32, DEP_STYLE),
    (226, "derived from", 22, 21, DEP_STYLE),
    (227, "emits", 21, 23, DEP_STYLE),
    (228, "part of", 51, 52, COMP_STYLE),
    (229, "part of", 51, 53, COMP_STYLE),
    (230, "part of", 51, 54, COMP_STYLE),
    (231, "opens / closes", 62, 63, ASSOC_STYLE),
    (232, "fills", 62, 64, ASSOC_STYLE),
    (233, "updates", 64, 63, ASSOC_STYLE),
    (234, "evaluates against", 71, 73, DEP_STYLE),
    (235, "creates", 71, 72, DEP_STYLE),
    (236, "evaluates against", 81, 73, DEP_STYLE),
    (237, "may create", 81, 72, DEP_STYLE),
    (238, "may create", 81, 41, DEP_STYLE),
    (239, "routes to", 81, 51, AGGR_STYLE),
    (240, "part of", 91, 92, COMP_STYLE),
    (241, "part of", 91, 93, COMP_STYLE),
    (242, "part of", 91, 94, COMP_STYLE),
    (243, "optional broker", 91, 51, AGGR_STYLE),
    (244, "uses", 93, 21, DEP_STYLE),
    (245, "uses", 93, 34, DEP_STYLE),
    (246, "related trade", 94, 41, DEP_STYLE),
    (247, "uses", 103, 21, DEP_STYLE),
    (248, "uses", 103, 34, DEP_STYLE),
]

for item in edges:
    e(root, *item)

ElementTree(mxfile).write("docs/fintex-domain-model.drawio", encoding="utf-8", xml_declaration=False)
