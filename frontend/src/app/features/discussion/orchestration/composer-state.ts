
export interface ComposerState {
    busy: boolean;
    error: string | null;
    fields: Record<string, string[]>;
}
