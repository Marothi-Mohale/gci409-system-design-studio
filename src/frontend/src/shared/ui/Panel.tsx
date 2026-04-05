import { PropsWithChildren } from "react";

type PanelProps = PropsWithChildren<{
  title?: string;
  subtitle?: string;
  actions?: React.ReactNode;
  className?: string;
}>;

export function Panel({ title, subtitle, actions, className, children }: PanelProps) {
  return (
    <section className={`panel ${className ?? ""}`.trim()}>
      {(title || actions) && (
        <div className="panel-header">
          <div>
            {title && <h3>{title}</h3>}
            {subtitle && <p className="subtle-text">{subtitle}</p>}
          </div>
          {actions && <div className="button-row">{actions}</div>}
        </div>
      )}
      {children}
    </section>
  );
}
