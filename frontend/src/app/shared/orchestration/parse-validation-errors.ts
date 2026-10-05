import { isRecord } from "./is-record";

export function parseValidationErrors(value: unknown): Record<string, string[]> {
    if (!isRecord(value)) return {};
    return Object.fromEntries(Object.entries(value).slice(0, 50).flatMap(([field, messages]) => {
        if (!Array.isArray(messages)) return [];
        const safeMessages = messages.filter((message): message is string =>
            typeof message === "string" && message.length <= 2048).slice(0, 20);
        return safeMessages.length ? [[field, safeMessages]] : [];
    }));
}
