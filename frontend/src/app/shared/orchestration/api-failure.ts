export class ApiFailure extends Error {
    constructor(
        public readonly status: number,
        message: string,
        public readonly fields: Record<string, string[]> = {},
        public readonly retryAfterSeconds: number | null = null,
    ) {
        super(message);
    }
}
