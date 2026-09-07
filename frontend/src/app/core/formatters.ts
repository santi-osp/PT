import { Center, Customer, Neighborhood } from './models';

export function displayName(customer: Customer): string {
  return customer.treatment === 'Empresa' ? customer.extendedLegalName : customer.fullName;
}

export function documentLabel(customer: Customer): string {
  return `${customer.documentType} ${customer.documentNumber}${customer.verificationDigit ? `-${customer.verificationDigit}` : ''}`;
}

export function centerLabel(centerId: string, centers: Center[]): string {
  const center = centers.find((item) => item.id === centerId);
  return center ? `${center.code} · ${center.name}` : centerId;
}

export function geography(neighborhoodId: string, neighborhoods: Neighborhood[]): Neighborhood | undefined {
  return neighborhoods.find((item) => item.id === neighborhoodId);
}

export function parseLegalName(value: string): { firstNames: string; lastNames: string; fullName: string } {
  const tokens = value.trim().split(/\s+/).filter(Boolean);
  if (!tokens.length) return { firstNames: '', lastNames: '', fullName: '' };
  const firstCount = tokens.length === 1 ? 1 : tokens.length === 2 ? 1 : tokens.length === 3 ? 2 : tokens.length - 2;
  return {
    firstNames: tokens.slice(0, firstCount).join(', '),
    lastNames: tokens.slice(firstCount).join(', '),
    fullName: tokens.join(' ')
  };
}
