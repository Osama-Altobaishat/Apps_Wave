import { useEffect, useState } from "react";
import api from "./api";

interface Booking {
  id: number;
  resourceName: string;
  startAt: string;
  endAt: string;
  status: number;
  notes: string;
}
interface Resource {
  id: number;
  name: string;
  description: string;
}

export default function UserBookings() {
  const [resources, setResources] = useState<Resource[]>([]);
  const [selectedResource, setSelectedResource] = useState<Resource | null>(
    null,
  );
  const loadBookingsByResource = async (resourceId: number) => {
    const res = await api.get(`api/bookings/filter/?resourceId=${resourceId}`);
    setBookings(res.data);
  };

  useEffect(() => {
    const fetchData = async () => {
      const res = await api.get(`api/resources/User`);
      setResources(res.data);
    };

    fetchData();
  }, []);

  const [bookings, setBookings] = useState<Booking[]>([]);

  const [resourceId, setResourceId] = useState(1);

  const [startAt, setStartAt] = useState("");

  const [endAt, setEndAt] = useState("");

  const [notes, setNotes] = useState("");

  const loadBookings = async (id: number) => {
    const res = await api.get(`api/bookings/filter?resourceId=${id}`);

    setBookings(res.data);
  };

  const [message, setMessage] = useState<string>("");
  const [error, setError] = useState<string>("");

  const createBooking = async (id: number) => {
    try {
      setError("");
      setMessage("");

      await api.post("api/bookings", {
        resourceId,
        startAt,
        endAt,
        notes,
      });

      setMessage("Booking created successfully");
      setNotes("");
      loadBookings(id);
    } catch (err: any) {
      const msg = err?.response?.data?.message || "Something went wrong";

      setError(msg);
    }
  };

  const cancelBooking = async (id: number) => {
    await api.put(`api/bookings/Cancelled/${id}`);

    loadBookings(resourceId);
  };

  return (
    <div className="container mt-5">
      <h2>Bookings</h2>
      <div className="card shadow-sm border-0 mb-4">
        <div className="card-header bg-white border-0">
          <h5 className="mb-0">Available Resources</h5>
        </div>

        <div className="card-body">
          <div className="row g-3">
            {resources.map((r) => {
              const isSelected = selectedResource?.id === r.id;

              return (
                <div className="col-md-4" key={r.id}>
                  <div
                    className={`p-3 rounded-3 border h-100 resource-card ${
                      isSelected
                        ? "border-primary bg-primary-subtle"
                        : "bg-white"
                    }`}
                    onClick={() => {
                      setSelectedResource(r);
                      setResourceId(r.id);
                    }}
                    style={{
                      cursor: "pointer",
                      transition: "all 0.2s ease-in-out",
                    }}
                  >
                    <div className="d-flex justify-content-between align-items-start">
                      <div>
                        <h6 className="mb-1 fw-bold">{r.name}</h6>
                        <p className="text-muted small mb-2">{r.description}</p>
                      </div>

                      {isSelected && (
                        <span className="badge bg-primary">Selected</span>
                      )}
                    </div>

                    <div className="d-flex justify-content-end mt-3">
                      <button
                        className="btn btn-sm btn-outline-primary"
                        onClick={(e) => {
                          e.stopPropagation();
                          setResourceId(r.id);
                          loadBookingsByResource(r.id);
                        }}
                      >
                        View Bookings
                      </button>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>

      {message && <div className="alert alert-success">{message}</div>}

      {error && <div className="alert alert-danger">{error}</div>}

      <div className="card shadow-sm border-0 mb-4">
        <div className="card-header bg-white border-0">
          <h5 className="mb-6">Time Resources booking</h5>

          <div style={{ maxHeight: "180px", overflowY: "auto" }}>
            <table className="table table-striped mb-0">
              <thead>
                <tr>
                  <th>Resource</th>
                  <th>Start</th>
                  <th>End</th>
                  <th>Status</th>
                  <th></th>
                </tr>
              </thead>

              <tbody>
                {bookings.map((b) => (
                  <tr key={b.id}>
                    <td>{b.resourceName}</td>
                    <td>{new Date(b.startAt).toLocaleString()}</td>
                    <td>{new Date(b.endAt).toLocaleString()}</td>
                    <td>
                      {b.status === 0
                        ? "Pending"
                        : b.status === 1
                          ? "Confirmed"
                          : b.status === 2
                            ? "Cancelled"
                            : b.status === 3
                              ? "Completed"
                              : "Unknown"}
                    </td>

                    <td>
                      <button
                        className="btn btn-danger"
                        onClick={() => cancelBooking(b.id)}
                      >
                        Cancel
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <div className="card p-4 mb-4">
        <div className="mb-3">
          <h5 className="mb-6">Create Booking</h5>
          <label>Resource Id</label>

          <input
            className="form-control"
            type="text"
            value={selectedResource?.name || ""}
            disabled
          />
        </div>

        <div className="mb-3">
          <label>Start</label>

          <input
            className="form-control"
            type="datetime-local"
            value={startAt}
            onChange={(e) => setStartAt(e.target.value)}
          />
        </div>

        <div className="mb-3">
          <label>End</label>

          <input
            className="form-control"
            type="datetime-local"
            value={endAt}
            onChange={(e) => setEndAt(e.target.value)}
          />
        </div>

        <div className="mb-3">
          <label>Notes</label>

          <textarea
            className="form-control"
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
          />
        </div>

        <button
          className="btn btn-primary"
          onClick={() => createBooking(resourceId)}
        >
          Create Booking
        </button>
      </div>
    </div>
  );
}
