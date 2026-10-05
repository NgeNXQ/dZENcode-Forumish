
export interface AttachmentState {
    loading: boolean;
    error: string | null;
    imageUrl: string | null;
    text: string | null;
    openUrl: string;
    unavailable: boolean;
}
