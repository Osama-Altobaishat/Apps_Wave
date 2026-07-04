import { useEffect, useState } from "react";
import api from "./api";
import { logout } from "./Logout";

interface Resource {
  id: number;
  name: string;
  description: string;
  isActive: boolean;
  createdAt: string;
}

export default function ResourcesPage() {
  const [resources, setResources] = useState<Resource[]>([]);
  const [onlyActive, setOnlyActive] = useState(true);

  const [showModal, setShowModal] = useState(false);
  const [editId, setEditId] = useState<number | null>(null);

  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [isActive, setIsActive] = useState(true);

  // Load resources
  const loadResources = async () => {
    const res = await api.get(
      `api/resources/dashboard?onlyActive=${onlyActive}`,
    );
    setResources(res.data);
  };

  // Reset form
  const resetForm = () => {
    setName("");
    setDescription("");
    setIsActive(true);
    setEditId(null);
  };

  // Open create modal
  const openCreate = () => {
    resetForm();
    setShowModal(true);
  };

  // Open edit modal
  const openEdit = (r: Resource) => {
    setEditId(r.id);
    setName(r.name);
    setDescription(r.description);
    setIsActive(r.isActive);
    setShowModal(true);
  };

  // Save (create or update)
  const saveResource = async () => {
    if (editId === null) {
      await api.post("api/resources", {
        name,
        description,
      });
    } else {
      await api.put(`api/resources/${editId}`, {
        name,
        description,
        isActive,
      });
    }

    setShowModal(false);
    resetForm();
    loadResources();
  };

  // Delete
  const deleteResource = async (id: number) => {
    await api.delete(`api/resources/${id}`);
    loadResources();
  };

  const toggleResourceActive = async (id: number, current: boolean) => {
    try {
      await api.put(`api/resources/active/${id}`, {
        isActive: !current,
      });

      // تحديث UI مباشرة بدون إعادة تحميل
      setResources((prev) =>
        prev.map((r) => (r.id === id ? { ...r, isActive: !current } : r)),
      );
    } catch (error) {
      console.error("Error toggling resource status:", error);
    }
  };
  useEffect(() => {
    const fetchData = async () => {
      const res = await api.get(
        `api/resources/dashboard?onlyActive=${onlyActive}`,
      );
      setResources(res.data);
    };

    fetchData();
  }, [onlyActive]);

  return (
    <div className="container mt-4">
      <div className="d-flex justify-content-between align-items-center mb-3">

          <h1>Dashboard</h1>

          <button onClick={logout} className="btn btn-outline-danger btn-sm">
            Logout
          </button>

      </div>
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h3>Resources</h3>

        <div>
          <button className="btn btn-primary me-2" onClick={openCreate}>
            + Add Resource
          </button>

          <select
            className="form-select d-inline w-auto"
            value={onlyActive ? "active" : "all"}
            onChange={(e) => setOnlyActive(e.target.value === "active")}
          >
            <option value="active">Active Only</option>
            <option value="all">All</option>
          </select>
        </div>
      </div>

      <table className="table table-bordered">
        <thead>
          <tr>
            <th>Name</th>
            <th>Description</th>
            <th>Status</th>
            <th>Created At</th>
            <th>Actions</th>
          </tr>
        </thead>

        <tbody>
          {resources.map((r) => (
            <tr key={r.id}>
              <td>{r.name}</td>
              <td>{r.description}</td>
              <td>
                {r.isActive ? (
                  <span className="badge bg-success">Active</span>
                ) : (
                  <span className="badge bg-secondary">Inactive</span>
                )}
              </td>
              <td>{new Date(r.createdAt).toLocaleString()}</td>

              <td>
                <button
                  className={`btn btn-sm me-2 ${
                    r.isActive ? "btn-success" : "btn-secondary"
                  }`}
                  onClick={() => toggleResourceActive(r.id, r.isActive)}
                >
                  {r.isActive ? "Active" : "Inactive"}
                </button>

                <button
                  className="btn btn-warning btn-sm me-2"
                  onClick={() => openEdit(r)}
                >
                  Edit
                </button>

                <button
                  className="btn btn-danger btn-sm"
                  onClick={() => deleteResource(r.id)}
                >
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {/* Modal */}
      {showModal && (
        <div className="modal d-block" style={{ background: "#00000066" }}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  {editId ? "Edit Resource" : "Create Resource"}
                </h5>
                <button
                  className="btn-close"
                  onClick={() => setShowModal(false)}
                />
              </div>

              <div className="modal-body">
                <input
                  className="form-control mb-2"
                  placeholder="Name"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                />

                <textarea
                  className="form-control mb-2"
                  placeholder="Description"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                />

                {editId !== null && (
                  <div className="form-check">
                    <input
                      type="checkbox"
                      className="form-check-input"
                      checked={isActive}
                      onChange={(e) => setIsActive(e.target.checked)}
                    />
                    <label className="form-check-label">Active</label>
                  </div>
                )}
              </div>

              <div className="modal-footer">
                <button
                  className="btn btn-secondary"
                  onClick={() => setShowModal(false)}
                >
                  Cancel
                </button>

                <button className="btn btn-primary" onClick={saveResource}>
                  Save
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
