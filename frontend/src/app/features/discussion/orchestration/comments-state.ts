import { Comment } from "./comment";

export interface CommentsState {
    items: Comment[];
    loading: boolean;
    error: string | null;
    page: number;
    size: number;
    hasNext: boolean;
}
