import { isRecord } from "../../../shared/orchestration/is-record";
import { isWebUrl } from "../../../shared/orchestration/is-web-url";
import { Comment } from "../orchestration/comment";

export function parseComment(value: unknown): Comment {
    if (!isRecord(value)) throw new Error("Invalid comment response.");
    const id = value['id'];
    const parentId = value['parentId'];
    const email = value['email'];
    const username = value['username'];
    const homePage = value['homePage'];
    const message = value['message'];
    const attachmentId = value['attachmentId'];
    const createdAt = value['createdAt'];
    if (typeof id !== "number" || !Number.isSafeInteger(id) || id <= 0 ||
        !(parentId === null || (typeof parentId === "number" && Number.isSafeInteger(parentId) && parentId > 0 && parentId !== id)) ||
        typeof email !== "string" || !/^[^\s@?&#]+@[^\s@?&#]+$/.test(email) ||
        typeof username !== "string" || typeof message !== "string" ||
        !(homePage === null || (typeof homePage === "string" && isWebUrl(homePage))) ||
        !(attachmentId === null || (typeof attachmentId === "string" && /^[A-Za-z0-9-]+$/.test(attachmentId))) ||
        typeof createdAt !== "string" || !Number.isFinite(Date.parse(createdAt))) {
        throw new Error("Invalid comment response.");
    }
    return { id, parentId, email, username, homePage, message, attachmentId, createdAt };
}
