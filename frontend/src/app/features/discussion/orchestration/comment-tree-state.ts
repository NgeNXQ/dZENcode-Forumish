import { Comment } from "./comment";

export interface CommentTreeState {
    expanded: boolean;
    loaded: boolean;
    loading: boolean;
    error: string | null;
    items: Comment[];
    page: number;
    hasNext: boolean;
}
