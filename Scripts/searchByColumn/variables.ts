export let id_count = 1;
export let countRemove = 1;

export function incrementIdCount(): void {
    id_count++;
}

export function incrementCountRemove(): void {
    countRemove++;
}

export function decreaseCountRemove(): void {
    countRemove--;
}

export function resetCounters(): void {
    id_count = 1;
    countRemove = 1;
}