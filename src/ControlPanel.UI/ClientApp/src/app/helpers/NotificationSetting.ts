import Machine from "./Machine";

export interface NotificationSetting {
  machine: Machine;
  metric: string;
  threshold: number;
  interval: number;
  lastNotificationTimestamp?: number;
}
