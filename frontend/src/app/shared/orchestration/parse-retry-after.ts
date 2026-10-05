export function parseRetryAfter(value: string | null, now = Date.now()): number | null {
    if (value === null || !value.trim()) return null;
    if (/^\d+$/.test(value.trim())) {
        const seconds = Number(value);
        return Number.isSafeInteger(seconds) ? seconds : null;
    }
    if (!/^(Mon|Tue|Wed|Thu|Fri|Sat|Sun), \d{2} (Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec) \d{4} \d{2}:\d{2}:\d{2} GMT$/.test(value.trim())) return null;
    const date = Date.parse(value);
    return Number.isFinite(date) ? Math.max(0, Math.ceil((date - now) / 1000)) : null;
}
