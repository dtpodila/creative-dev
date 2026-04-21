export interface BillSummary {
  billId: number;
  billFileName: string;
  billingPeriod?: string;
  billingDate?: string;
  vendorName?: string;
  accountNumber?: string;
  totalBillAmount: number;
  numberOfLines: number;
  parseStatus: "Pending" | "Processing" | "Parsed" | "Failed";
  createdAt: string;
}

export interface PlanCharge {
  planChargeId: number;
  chargeName: string;
  chargeAmount: number;
  chargeType?: string;
}

export interface EquipmentCharge {
  equipmentChargeId: number;
  deviceName?: string;
  chargeName: string;
  chargeAmount: number;
  chargeType?: string;
}

export interface ServiceCharge {
  serviceChargeId: number;
  chargeName: string;
  chargeAmount: number;
  chargeType?: string;
}

export interface LineDetail {
  lineId: number;
  phoneNumber: string;
  lineLabel?: string;
  assignedName?: string;
  assignedContact?: string;
  planCostShare: number;
  equipmentCost: number;
  servicesCost: number;
  totalLineCost: number;
  equipmentCharges: EquipmentCharge[];
  serviceCharges: ServiceCharge[];
}

export interface BillDetail {
  billId: number;
  billingPeriod?: string;
  billingDate?: string;
  accountNumber?: string;
  vendorName?: string;
  totalBillAmount: number;
  totalPlanAmount: number;
  totalEquipmentAmount: number;
  totalServicesAmount: number;
  numberOfLines: number;
  parseStatus: string;
  lines: LineDetail[];
  planCharges: PlanCharge[];
}
