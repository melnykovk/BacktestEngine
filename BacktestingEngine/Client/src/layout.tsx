import "./css/base/layout.css";
import { NavLink } from "react-router-dom";
export function Layout({ children }: {children: React.ReactNode }) {
    return(
          
        <div className="app-shell">
        <div className="app-layout">
            <aside className="sidebar">
                <h2>Menu</h2>

                <nav className="nav">
    <NavLink
        to="/testing"
        className={({ isActive }) =>
            isActive ? "nav-button active" : "nav-button"
        }
    >
        Testing
    </NavLink>

    <NavLink
        to="/configmanager"
        className={({ isActive }) =>
            isActive ? "nav-button active" : "nav-button"
        }
    >
        Backtest Config
    </NavLink>

    <NavLink
        to="/dbmanager"
        className={({ isActive }) =>
            isActive ? "nav-button active" : "nav-button"
        }
    >
        Dataset Information
    </NavLink>


</nav>
            </aside>
            {children}
        </div>
    </div>
    )
}