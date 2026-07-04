import { useEffect, useState } from "react";
import api from "./api";

interface User {
  id: number;
  fullName: string;
  email: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}

export default function UserInfo() {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState("");

  const loadUsers = async () => {
    try {
      const res = await api.get("/api/users");
      setUsers(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

useEffect(() => {
  const fetchData = async () => {
    try {
      const res = await api.get("/api/users");
      setUsers(res.data);
    } catch (error) {
      console.error("Error:", error);
    } finally {
      console.log("Finished");
      setLoading(false);
    }
  };

  fetchData();
}, []);

  const changeRole = async (id: number, role: number) => {
  try {
    await api.put(`/api/users/role/${id}`, {
      role: role,
    });

    setMessage("Role updated successfully.");
    loadUsers();
  } catch (err) {
    console.error(err);
    setMessage("Failed to update role.");
  }
};

  if (loading)
    return <div className="container mt-4">Loading...</div>;

  return (
    <div className="container mt-4">
      <h2>Users</h2>

      {message && (
        <div className="alert alert-info">
          {message}
        </div>
      )}

      <table className="table table-bordered table-striped">
        <thead>
          <tr>
            <th>#</th>
            <th>Full Name</th>
            <th>Email</th>
            <th>Role</th>
            <th>Status</th>
            <th>Created</th>
            <th>Change Role</th>
          </tr>
        </thead>

        <tbody>
          {users.map((user) => (
            <tr key={user.id}>
              <td>{user.id}</td>

              <td>{user.fullName}</td>

              <td>{user.email}</td>

              <td>
                <span className="badge bg-primary">
                  {user.role}
                </span>
              </td>

              <td>
                {user.isActive ? (
                  <span className="badge bg-success">
                    Active
                  </span>
                ) : (
                  <span className="badge bg-danger">
                    Inactive
                  </span>
                )}
              </td>

              <td>
                {new Date(user.createdAt).toLocaleDateString()}
              </td>

              <td>
                <select
                  className="form-select"
                  value={user.role}
                  onChange={(e) =>
                    changeRole(user.id, Number(e.target.value))
                  }
                >
                    <option value={0}>User</option>
                    <option value={1}>Admin</option>
                </select>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}