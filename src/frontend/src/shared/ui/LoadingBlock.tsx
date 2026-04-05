export function LoadingBlock({ label }: { label: string }) {
  return (
    <div className="empty-state" aria-busy="true">
      <strong>{label}</strong>
      <p className="subtle-text">Fetching the latest workspace data.</p>
    </div>
  );
}
