export interface Draft {
    parentId: number | null;
    email: string;
    username: string;
    homePage: string;
    message: string;
    attachment: File | null;
}
