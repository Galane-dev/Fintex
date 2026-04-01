"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { Form, message } from "antd";
import { useExternalBrokerAccounts } from "@/hooks/useExternalBrokerAccounts";
import { useLiveTrading } from "@/hooks/useLiveTrading";
import { usePaperTrading } from "@/hooks/usePaperTrading";
import { useAcademyStatus } from "@/hooks/use-academy-status";
import { getActionableRecommendationAction } from "@/utils/paper-trading";
import {
  EXTERNAL_BROKER_PLATFORM_DIRECT_API,
  EXTERNAL_BROKER_PROVIDER_ALPACA,
} from "@/types/external-broker";
import {
  PAPER_EXECUTION_TARGET,
  getExternalConnectionIdFromTarget,
  isPaperExecutionTarget,
} from "./constants";
import { getExternalConnectionStatusTone } from "./helpers";
import {
  buildAccountMetrics,
  buildExecutionTargets,
  getConnectedExternalConnections,
  sortConnections,
} from "./panel-derived";
import type {
  AccountFormValues,
  DashboardPaperTradingActions,
  ExternalBrokerFormValues,
  PaperTradingPanelController,
  PaperTradingPanelProps,
  TradeFormValues,
} from "./types";

export const usePaperTradingPanelController = ({
  registerDashboardActions,
}: Pick<PaperTradingPanelProps, "registerDashboardActions">): PaperTradingPanelController => {
  const [accountForm] = Form.useForm<AccountFormValues>();
  const [tradeForm] = Form.useForm<TradeFormValues>();
  const [externalBrokerForm] = Form.useForm<ExternalBrokerFormValues>();
  const [tradeDirection, setTradeDirection] = useState<"Buy" | "Sell">("Buy");
  const [isAccountsOpen, setIsAccountsOpen] = useState(false);
  const [isTradeOpen, setIsTradeOpen] = useState(false);
  const [isRecommendationOpen, setIsRecommendationOpen] = useState(false);
  const [isAssessmentOpen, setIsAssessmentOpen] = useState(false);
  const [isRecommendationLoading, setIsRecommendationLoading] = useState(false);
  const [recommendationRequestError, setRecommendationRequestError] = useState<string | null>(null);
  const paperTrading = usePaperTrading();
  const externalBrokers = useExternalBrokerAccounts();
  const liveTrading = useLiveTrading();
  const academy = useAcademyStatus();
  const account = paperTrading.snapshot?.account ?? null;
  const positions = paperTrading.snapshot?.positions ?? [];
  const orders = paperTrading.snapshot?.recentOrders ?? [];
  const fills = paperTrading.snapshot?.recentFills ?? [];
  const watchedQuantity = Form.useWatch("quantity", tradeForm);
  const sortedConnections = useMemo(() => sortConnections(externalBrokers.connections), [externalBrokers.connections]);
  const connectedExternalConnections = useMemo(() => getConnectedExternalConnections(sortedConnections), [sortedConnections]);
  const canConnectExternalBrokers = academy.status?.canConnectExternalBrokers ?? false;
  const availableExecutionTargets = useMemo(
    () => buildExecutionTargets(account, connectedExternalConnections, canConnectExternalBrokers),
    [account, canConnectExternalBrokers, connectedExternalConnections],
  );
  const accountMetrics = useMemo(() => buildAccountMetrics(account), [account]);
  const hasTradingAccess = account != null || connectedExternalConnections.length > 0;
  const requestRecommendationRef = useRef<() => Promise<void>>(async () => undefined);
  const availableTargetsRef = useRef(availableExecutionTargets);
  const hasTradingAccessRef = useRef(hasTradingAccess);

  const requestRecommendation = useCallback(async () => {
    setRecommendationRequestError(null);
    setIsRecommendationLoading(true);
    const values = tradeForm.getFieldsValue();
    const result = await paperTrading.getRecommendation({
      symbol: "BTCUSDT",
      assetClass: 1,
      provider: 1,
      quantity: values.quantity ?? null,
      stopLoss: values.stopLoss ?? null,
      takeProfit: values.takeProfit ?? null,
    });

    if (result) {
      const suggestedTradeAction = getActionableRecommendationAction(result);
      tradeForm.setFieldsValue({
        stopLoss: values.stopLoss ?? result.suggestedStopLoss ?? undefined,
        takeProfit: values.takeProfit ?? result.suggestedTakeProfit ?? undefined,
      });
      if (suggestedTradeAction) {
        setTradeDirection(suggestedTradeAction);
      }
    } else {
      setRecommendationRequestError("We could not load a recommendation right now. Please try again.");
    }

    setIsRecommendationLoading(false);
  }, [paperTrading, tradeForm]);

  const openAccountsModal = useCallback(() => {
    setIsAccountsOpen(true);
  }, []);

  const openTradeModal = useCallback(
    (direction: "Buy" | "Sell") => {
      setTradeDirection(direction);

      if (!account && connectedExternalConnections.length === 0) {
        setIsAccountsOpen(true);
        message.info("Create your paper account or connect Alpaca before placing trades.");
        return;
      }

      tradeForm.setFieldsValue({
        executionTarget:
          tradeForm.getFieldValue("executionTarget") ??
          availableExecutionTargets[0]?.value ??
          PAPER_EXECUTION_TARGET,
      });
      setIsTradeOpen(true);
    },
    [account, availableExecutionTargets, connectedExternalConnections.length, tradeForm],
  );

  const openRecommendationModal = useCallback(async () => {
    setIsRecommendationOpen(true);
    await requestRecommendation();
  }, [requestRecommendation]);

  useEffect(() => {
    requestRecommendationRef.current = requestRecommendation;
    availableTargetsRef.current = availableExecutionTargets;
    hasTradingAccessRef.current = hasTradingAccess;
  }, [availableExecutionTargets, hasTradingAccess, requestRecommendation]);

  const openAccountsAction = useCallback(() => {
    setIsAccountsOpen(true);
  }, []);

  const openRecommendationAction = useCallback(() => {
    setIsRecommendationOpen(true);
    void requestRecommendationRef.current();
  }, []);

  const openTradeAction = useCallback(
    (direction: "Buy" | "Sell") => {
      setTradeDirection(direction);

      if (!hasTradingAccessRef.current) {
        setIsAccountsOpen(true);
        message.info("Create your paper account or connect Alpaca before placing trades.");
        return;
      }

      tradeForm.setFieldsValue({
        executionTarget:
          tradeForm.getFieldValue("executionTarget") ??
          availableTargetsRef.current[0]?.value ??
          PAPER_EXECUTION_TARGET,
      });
      setIsTradeOpen(true);
    },
    [tradeForm],
  );

  useEffect(() => {
    if (!registerDashboardActions) {
      return;
    }

    const actions: DashboardPaperTradingActions = {
      hasAccount: hasTradingAccess,
      openAccounts: openAccountsAction,
      openRecommendation: openRecommendationAction,
      openTrade: openTradeAction,
    };

    registerDashboardActions(actions);
  }, [
    hasTradingAccess,
    openAccountsAction,
    openRecommendationAction,
    openTradeAction,
    registerDashboardActions,
  ]);

  const handleAccountCreate = useCallback(async (values: AccountFormValues) => {
    await paperTrading.createAccount(values);
    accountForm.resetFields();
    setIsAccountsOpen(false);
    message.success("Paper account created.");
  }, [accountForm, paperTrading]);

  const handleConnectExternalBroker = useCallback(async (values: ExternalBrokerFormValues) => {
    const connection = await externalBrokers.connectAccount({
      displayName: values.displayName,
      provider: EXTERNAL_BROKER_PROVIDER_ALPACA,
      platform: EXTERNAL_BROKER_PLATFORM_DIRECT_API,
      apiKey: values.apiKey,
      apiSecret: values.apiSecret,
      isPaperEnvironment: values.environment !== "live",
    });

    if (connection) {
      externalBrokerForm.resetFields();
      message.success("Alpaca account connected.");
    }
  }, [externalBrokerForm, externalBrokers]);

  const handleDisconnectExternalBroker = useCallback(async (id: number) => {
    await externalBrokers.disconnectAccount({ id });
    message.success("External broker disconnected.");
  }, [externalBrokers]);

  const submitTrade = useCallback(async (direction: "Buy" | "Sell") => {
    setTradeDirection(direction);
    const values = await tradeForm.validateFields();
    const orderInput = {
      symbol: "BTCUSDT",
      assetClass: 1,
      provider: 1,
      direction,
      quantity: values.quantity ?? 0.01,
      stopLoss: values.stopLoss ?? null,
      takeProfit: values.takeProfit ?? null,
      notes: values.notes ?? "",
    };

    if (isPaperExecutionTarget(values.executionTarget)) {
      const result = await paperTrading.placeOrder(orderInput);
      if (!result) return;
      if (result.wasExecuted) {
        tradeForm.resetFields(["quantity", "stopLoss", "takeProfit", "notes"]);
        setIsTradeOpen(false);
        setIsAssessmentOpen(false);
        message.success(`${direction} paper trade placed.`);
      } else {
        setIsAssessmentOpen(true);
      }
      return;
    }

    const connectionId = getExternalConnectionIdFromTarget(values.executionTarget);
    if (!connectionId) {
      message.warning("Select a valid execution destination.");
      return;
    }

    const execution = await liveTrading.placeOrder({ connectionId, ...orderInput });
    if (!execution) return;
    tradeForm.resetFields(["quantity", "stopLoss", "takeProfit", "notes"]);
    setIsTradeOpen(false);
    setIsAssessmentOpen(false);
    message.success(`${direction} live Alpaca order routed successfully.`);
  }, [liveTrading, paperTrading, tradeForm]);

  return {
    account,
    accountForm,
    accountMetrics,
    activeFeedback: paperTrading.latestAssessment,
    academyStatus: academy.status,
    availableExecutionTargets,
    canConnectExternalBrokers,
    combinedError: paperTrading.error ?? externalBrokers.error ?? liveTrading.error,
    effectiveQuantity: watchedQuantity ?? 0.01,
    externalBrokerForm,
    feedbackTone: paperTrading.latestAssessment == null ? null : paperTrading.latestAssessment.riskLevel === "High" ? "error" : paperTrading.latestAssessment.riskLevel === "Medium" ? "warning" : "success",
    fills,
    getConnectionStatusTone: getExternalConnectionStatusTone,
    handleAccountCreate,
    handleApplyAssessmentSuggestions: async () => {
      const assessment = paperTrading.latestAssessment;
      if (!assessment) return;
      const currentValues = tradeForm.getFieldsValue();
      tradeForm.setFieldsValue({
        quantity: currentValues.quantity ?? 0.01,
        stopLoss: assessment.suggestedStopLoss ?? currentValues.stopLoss ?? undefined,
        takeProfit: assessment.suggestedTakeProfit ?? currentValues.takeProfit ?? undefined,
      });
      await submitTrade(assessment.direction);
    },
    handleClearAnyError: () => {
      paperTrading.clearError();
      externalBrokers.clearError();
      liveTrading.clearError();
    },
    handleClosePaperPosition: async (positionId: number) => {
      await paperTrading.closePosition({ positionId });
    },
    handleConnectExternalBroker,
    handleDisconnectExternalBroker,
    handlePlaceSuggestedTrade: async () => {
      const recommendation = paperTrading.recommendation;
      if (!recommendation) return;
      const suggestedTradeAction = getActionableRecommendationAction(recommendation);
      if (!suggestedTradeAction) {
        message.info("This recommendation is a hold, so there is no trade to place yet.");
        return;
      }
      const values = tradeForm.getFieldsValue();
      tradeForm.setFieldsValue({
        quantity: values.quantity ?? 0.01,
        executionTarget: values.executionTarget ?? availableExecutionTargets[0]?.value ?? PAPER_EXECUTION_TARGET,
        stopLoss: recommendation.suggestedStopLoss ?? values.stopLoss ?? undefined,
        takeProfit: recommendation.suggestedTakeProfit ?? values.takeProfit ?? undefined,
      });
      if (isPaperExecutionTarget(values.executionTarget) && !account) {
        setIsRecommendationOpen(false);
        setIsAccountsOpen(true);
        message.info("Create your paper account before placing the suggested trade.");
        return;
      }
      await submitTrade(suggestedTradeAction);
      setIsRecommendationOpen(false);
    },
    isAccountsOpen,
    isAssessmentOpen,
    isAcademyLoading: academy.isLoading,
    isBusy: paperTrading.isSubmitting || liveTrading.isSubmitting,
    isExternalLoading: externalBrokers.isLoading,
    isExternalSubmitting: externalBrokers.isSubmitting,
    isLoading: paperTrading.isLoading,
    isRecommendationLoading,
    isRecommendationOpen,
    isSubmitting: paperTrading.isSubmitting,
    isTradeOpen,
    latestLiveExecution: liveTrading.lastExecution,
    liveTrades: liveTrading.trades,
    openAccountsModal,
    openRecommendationModal,
    openTradeModal,
    orders,
    positions,
    recommendation: paperTrading.recommendation,
    recommendationRequestError,
    recommendationTone: paperTrading.recommendation == null ? null : paperTrading.recommendation.riskLevel === "High" ? "error" : paperTrading.recommendation.riskLevel === "Medium" ? "warning" : "success",
    sortedConnections,
    submitTrade,
    closeAccountsModal: () => setIsAccountsOpen(false),
    closeAssessmentModal: () => setIsAssessmentOpen(false),
    closeRecommendationModal: () => {
      setIsRecommendationOpen(false);
      setRecommendationRequestError(null);
    },
    closeTradeModal: () => setIsTradeOpen(false),
    tradeDirection,
    tradeForm,
  };
};
