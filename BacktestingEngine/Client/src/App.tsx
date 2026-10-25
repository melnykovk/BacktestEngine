import { Routes, Route } from "react-router-dom";
import { Layout } from "./layout";
import { DbManager } from "./dbmanager/dbmanager";
import { ConfigManager } from "./configmanager/configmanager";

export function App() {
  return (
        <Layout>
            <Routes>
                <Route path="/configmanager" element={<ConfigManager />} />
                <Route path="/dbmanager" element={<DbManager />} />
            </Routes>
        </Layout>
    );
  }
export default App
