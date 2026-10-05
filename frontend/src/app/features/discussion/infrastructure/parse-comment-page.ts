import { isRecord } from "../../../shared/orchestration/is-record";
import { Comment } from "../orchestration/comment";
import { Page } from "../orchestration/page";
import { Reading } from "../orchestration/reading";
import { parseComment } from "./parse-comment";

export function parseCommentPage(value: unknown, reading: Reading, parentId: number | null): Page<Comment> {
    if (!isRecord(value) || !Array.isArray(value['items']) ||
        value['page'] !== reading.page || value['size'] !== reading.size ||
        value['items'].length > reading.size) {
        throw new Error("Invalid comments response.");
    }
    const items = value['items'].map(parseComment);
    if (items.some(item => item.parentId !== parentId) || new Set(items.map(item => item.id)).size !== items.length) {
        throw new Error("Invalid comment hierarchy.");
    }
    return { items, page: reading.page, size: reading.size };
}
