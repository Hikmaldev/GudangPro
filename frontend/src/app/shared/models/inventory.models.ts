export interface NavItem { label: string; route: string; icon: string; adminOnly?: boolean; badge?: number; }
export interface Item { code: string; name: string; category: string; unit: string; minStock: number; status: 'Aktif' | 'Menipis' | 'Nonaktif'; }
export interface Warehouse { code: string; name: string; address: string; itemCount: number; status: 'Aktif' | 'Nonaktif'; }
export interface Transaction { no: string; type: 'Masuk' | 'Keluar' | 'Koreksi'; date: string; warehouse: string; reference: string; items: string; creator: string; status: string; }
