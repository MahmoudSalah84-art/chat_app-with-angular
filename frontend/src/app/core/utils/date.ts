export function toUtcMs(value?: string | null): number {
  if (!value) return 0;
  const hasZone = /(Z|[+-]\d{2}:?\d{2})$/i.test(value);
  return new Date(hasZone ? value : `${value}Z`).getTime();
}