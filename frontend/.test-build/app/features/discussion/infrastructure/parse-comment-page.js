"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.parseCommentPage = parseCommentPage;
const is_record_1 = require("../../../shared/orchestration/is-record");
const parse_comment_1 = require("./parse-comment");
function parseCommentPage(value, reading, parentId) {
    if (!(0, is_record_1.isRecord)(value) || !Array.isArray(value['items']) ||
        value['page'] !== reading.page || value['size'] !== reading.size ||
        value['items'].length > reading.size) {
        throw new Error("Invalid comments response.");
    }
    const items = value['items'].map(parse_comment_1.parseComment);
    if (items.some(item => item.parentId !== parentId) || new Set(items.map(item => item.id)).size !== items.length) {
        throw new Error("Invalid comment hierarchy.");
    }
    return { items, page: reading.page, size: reading.size };
}
