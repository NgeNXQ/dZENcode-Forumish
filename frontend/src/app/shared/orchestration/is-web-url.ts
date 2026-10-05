export function isWebUrl(value: string): boolean {
    try {
        const url = new URL(value);
        return (url.protocol === "http:" || url.protocol === "https:") &&
            !url.username && !url.password;
    } catch {
        return false;
    }
}
