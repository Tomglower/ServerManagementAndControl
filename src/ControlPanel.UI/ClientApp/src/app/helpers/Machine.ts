export default interface Machine {
    id: number;
    link: string;
    data:string
    load:string |null;
    cpuUsage: string | null; 
    memoryUsage: string | null; 
    memoryFull: string |null
    diskUsage: string | null; 
    diskFull:string |null;
    networkTransmit:string|null;
    networkReceive:string|null;
  }
