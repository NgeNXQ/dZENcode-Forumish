export interface Comment {
    id: number;
    parentId: number | null;
    email: string;
    username: string;
    homePage: string | null;
    message: string;
    attachmentId: string | null;
    createdAt: string;
}
