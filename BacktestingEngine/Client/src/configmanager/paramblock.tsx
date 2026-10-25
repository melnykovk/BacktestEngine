import type { ReactNode } from "react";
import styles from "./css/configmanager.module.css";
import type { ParamConfig, ParamMode, ParamState } from "./hooks/useconfigmanager";

const MODES: { value: ParamMode; label: string }[] = [
    { value: "single", label: "Single" },
    { value: "range", label: "Range" },
    { value: "list", label: "List" }
];

function Row({ label, children }: { label: string; children: ReactNode }) {
    return (
        <div className={styles["range-row"]}>
            <label>{label}</label>
            {children}
        </div>
    );
}

interface Props {
    config: ParamConfig;
    state: ParamState;
    onChange: (next: ParamState) => void;
}

export function ParamBlock({ config, state, onChange }: Props) {
    const set = (patch: Partial<ParamState>) => onChange({ ...state, ...patch });

    return (
        <div className={styles["param-block"]}>
            <h3>{config.label}</h3>

            <div className={styles["mode-switch"]}>
                {MODES.map(m => (
                    <label key={m.value}>
                        <input
                            type="radio"
                            name={`${config.name}Mode`}
                            checked={state.mode === m.value}
                            onChange={() => set({ mode: m.value })}
                        />
                        {" "}{m.label}
                    </label>
                ))}
            </div>

            {state.mode === "single" && (
                <Row label="Value">
                    <input type="number" step={config.step}
                           value={state.value}
                           onChange={e => set({ value: e.target.value })} />
                </Row>
            )}

            {state.mode === "range" && (
                <>
                    <Row label="From">
                        <input type="number" step={config.step}
                               value={state.from}
                               onChange={e => set({ from: e.target.value })} />
                    </Row>
                    <Row label="To">
                        <input type="number" step={config.step}
                               value={state.to}
                               onChange={e => set({ to: e.target.value })} />
                    </Row>
                    <Row label="Step">
                        <input type="number" step={config.step}
                               value={state.step}
                               onChange={e => set({ step: e.target.value })} />
                    </Row>
                </>
            )}

            {state.mode === "list" && (
                <Row label="Values">
                    <input type="text"
                           value={state.list}
                           onChange={e => set({ list: e.target.value })} />
                </Row>
            )}
        </div>
    );
}