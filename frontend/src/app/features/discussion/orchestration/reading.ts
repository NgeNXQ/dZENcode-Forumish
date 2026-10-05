import { SortField } from "./sort-field";
import { SortDirection } from "./sort-direction";

export interface Reading {
    page: number;
    size: number;
    sortBy: SortField;
    direction: SortDirection;
}
