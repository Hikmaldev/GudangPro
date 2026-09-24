/** Model domain sesuai PRD bagian 10.3 dan 10.4. */

export type Role = 'Admin Gudang' | 'Staf Gudang' | 'Pemilik';
export type TransactionType = 'IN' | 'OUT' | 'ADJUST';

export interface User {
  id: number;
  username: string;
  fullName: string;
  role: Role;
  isActive: boolean;
  warehouseIds: number[];
  lastLoginAt: string;
}

export interface Warehouse {
  id: number;
  code: string;
  name: string;
  address: string;
  isActive: boolean;
  itemCount: number;
}

export interface Category { id: number; name: string; }
export interface Unit { id: number; name: string; }

export interface Item {
  id: number;
  code: string;
  name: string;
  categoryId: number;
  categoryName: string;
  unitId: number;
  unitName: string;
  minStock: number;
  quantity: number;
  isActive: boolean;
  warehouseName: string;
}

export interface TransactionLine { itemCode: string; itemName: string; quantity: number; unitName: string; available: number; }

export interface StockTransaction {
  id: number;
  transactionNo: string;
  type: TransactionType;
  warehouseId: number;
  warehouseName: string;
  transactionDate: string;
  referenceNo: string;
  notes: string;
  createdBy: string;
  createdAt: string;
  status: 'Berhasil' | 'Dibatalkan';
  lines: TransactionLine[];
}

export type AlertLevel = 'kritis' | 'menipis';

export interface Alert {
  id: number;
  itemId: number;
  warehouseName: string;
  itemName: string;
  message: string;
  level: AlertLevel;
  isRead: boolean;
  createdAt: string;
}

export interface AuditLog {
  id: number;
  userFullName: string;
  username: string;
  action: string;
  entityName: string;
  detail: string;
  ipAddress: string;
  createdAt: string;
}

export interface StockCardEntry { date: string; transactionNo: string; reference: string; incoming: number; outgoing: number; balance: number; createdBy: string; }

export interface StockCard {
  item: Item;
  openingBalance: number;
  totalIncoming: number;
  totalOutgoing: number;
  closingBalance: number;
  entries: StockCardEntry[];
}

export interface DashboardSummary {
  totalItems: number;
  transactionsToday: number;
  lowStockCount: number;
  stockValue: string;
  itemDelta: number;
  transactionDelta: number;
  valueDelta: number;
}

export interface CategoryStock { name: string; value: number; percentage: number; color: string; }
export interface TrendPoint { label: string; incoming: number; outgoing: number; }
