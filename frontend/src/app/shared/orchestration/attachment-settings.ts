export interface AttachmentSettings {
    mime: Record<string, number>;
    pollIntervalMs: number;
    pollAttempts: number;
}
