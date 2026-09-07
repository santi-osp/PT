export type Treatment = 'Sr' | 'Sra' | 'Empresa';
export type DocumentType = 'CC' | 'CE' | 'NIT';
export type TaxClass = 'PersonaNatural' | 'PersonaJuridica';
export type CustomerStatus = 'all' | 'active' | 'blocked';

export interface AddressRequest {
  neighborhoodId: string;
  isRural: boolean;
  ruralAddress: string | null;
  mainRoadType: string | null;
  mainRoadNumber: string | null;
  mainRoadLetter: string | null;
  mainRoadCardinality: string | null;
  secondaryRoadNumber1: string | null;
  secondaryRoadLetter: string | null;
  secondaryRoadCardinality1: string | null;
  secondaryRoadNumber2: string | null;
  secondaryRoadCardinality2: string | null;
}

export interface CreateCustomerRequest {
  treatment: Treatment;
  businessName: string;
  extendedLegalName: string;
  phone: string | null;
  phoneExtension: string | null;
  mobilePhone: string | null;
  email: string | null;
  documentType: DocumentType;
  documentNumber: string;
  verificationDigit: string | null;
  stratum: number;
  centerId: string;
  address: AddressRequest;
}

export interface UpdateCustomerRequest {
  businessName: string;
  firstNames: string | null;
  lastNames: string | null;
  phone: string | null;
  phoneExtension: string | null;
  mobilePhone: string | null;
  email: string | null;
  stratum: number;
  centerId: string;
  address: AddressRequest;
}

export interface Customer {
  id: string;
  code: string;
  treatment: Treatment;
  businessName: string;
  extendedLegalName: string;
  fullName: string;
  firstNames: string;
  lastNames: string;
  phone: string | null;
  phoneExtension: string | null;
  mobilePhone: string | null;
  email: string | null;
  documentType: DocumentType;
  documentNumber: string;
  verificationDigit: string | null;
  taxClass: TaxClass;
  paymentCondition: string;
  stratum: number;
  centerId: string;
  isBlocked: boolean;
  blockedAt: string | null;
  warning: string | null;
  address: AddressRequest & { formattedAddress: string };
  createdAt: string;
  updatedAt: string;
}

export interface Neighborhood {
  id: string;
  name: string;
  municipality: string;
  department: string;
  country: string;
  transportZone: string;
}

export interface Center { id: string; code: string; name: string }
export interface ProblemDetails { status?: number; title?: string; detail?: string; errors?: Record<string, string[]> }

export type AssistantResponseType = 'Text' | 'CustomerList' | 'CustomerDetail' | 'Count' | 'Confirmation' | 'Clarification' | 'Error';
export type AssistantIntent = 'HELP' | 'SEARCH_CUSTOMERS' | 'COUNT_CUSTOMERS' | 'GET_CUSTOMER' | 'CREATE_CUSTOMER' | 'UPDATE_CUSTOMER' | 'RETIRE_CUSTOMER' | 'UNKNOWN';

export interface AssistantFilters {
  searchText?: string | null;
  status?: string | null;
  neighborhood?: string | null;
  center?: string | null;
  stratum?: number | null;
  treatment?: string | null;
  documentType?: string | null;
}

export interface AssistantContext { customerId?: string | null; filters?: AssistantFilters | null }
export interface AssistantChangePreview { field: string; previousValue: string | null; proposedValue: string | null }
export interface PendingAssistantAction {
  token: string;
  actionType: AssistantIntent;
  targetCustomerId: string;
  customerCode: string;
  customerName: string;
  document: string;
  summary: string;
  changes: AssistantChangePreview[];
  expiresAt: string;
}
export interface AssistantCustomerCard { customer: Customer; neighborhood: string }
export interface AssistantMessageRequest { message: string; context?: AssistantContext | null }
export interface AssistantMessageResponse {
  responseType: AssistantResponseType;
  message: string;
  customers?: AssistantCustomerCard[] | null;
  count?: number | null;
  isTruncated: boolean;
  pendingAction?: PendingAssistantAction | null;
  context?: AssistantContext | null;
  openCreateForm: boolean;
}
