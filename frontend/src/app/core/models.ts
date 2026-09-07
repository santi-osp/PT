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
