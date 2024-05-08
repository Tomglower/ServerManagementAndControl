export default interface CustomMetric {
  id: string;
  name: string;
  query: string;
  machineId: number;
  value: number | null;
}
