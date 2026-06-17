export type DialogAlert = {
  kind: 'alert';
  id: number;
  title: string;
  message?: string;
  resolve: () => void;
};

export type DialogConfirm = {
  kind: 'confirm';
  id: number;
  title: string;
  message: string;
  confirmLabel: string;
  cancelLabel: string;
  destructive: boolean;
  resolve: (ok: boolean) => void;
};

export type DialogItem = DialogAlert | DialogConfirm;

type Listener = (item: DialogItem) => void;

let listener: Listener | null = null;
let nextId = 1;

export function subscribeDialog(fn: Listener): () => void {
  listener = fn;
  return () => {
    if (listener === fn) listener = null;
  };
}

export function enqueueDialog(item: Omit<DialogAlert, 'id'> | Omit<DialogConfirm, 'id'>): void {
  const withId = { ...item, id: nextId++ } as DialogItem;
  if (listener) {
    listener(withId);
    return;
  }
  throw new Error('Dialog host not mounted');
}

export function isDialogHostReady(): boolean {
  return listener != null;
}
