import {useEffect, useState } from "react";
import api from "./api";


interface Booking {
  id: number;
  userFullName: string;
  resourceName: string;
  startAt: string;
  endAt: string;
  status: number;
  notes: string;
}

export default function AdminBooking() {
  const [bookings, setBookings] = useState<Booking[]>([]);


  const loadBookings = async () => {
    const res = await api.get("api/bookings");
    setBookings(res.data);
  };

  const changeStatus = async (id: number, status: number) => {
    await api.put(`api/bookings/status/${id}`, {
      status,
    });

    loadBookings();
  };
  useEffect(() => {
  const fetchData = async () => {
    const res = await api.get(
      `api/bookings`
    );
    setBookings(res.data);
  };

  fetchData();
}, []);

  return (
    <div className="container mt-5">
      <table className="table table-bordered">
        <thead>
          <tr>
            <th>User</th>
            <th>Resource</th>
            <th>Start</th>
            <th>End</th>
            <th>Notes</th>
            <th>Status</th>
            <th>Action</th>
          </tr>
        </thead>

        <tbody>
          {bookings.map((b) => (
            <tr key={b.id}>
              <td>{b.userFullName}</td>
              <td>{b.resourceName}</td>
              <td>{b.notes}</td>
              <td>{new Date(b.startAt).toLocaleString()}</td>
              <td>{new Date(b.endAt).toLocaleString()}</td>

              <td>
                {b.status === 0
                  ? "Pending"
                  : b.status === 1
                    ? "Approved"
                    : b.status === 2
                      ? "Rejected"
                      : "Cancelled"}
              </td>

              <td>
                <button
                  className="btn btn-success me-2"
                  onClick={() => changeStatus(b.id, 1)}
                >
                  Approve
                </button>

                <button
                  className="btn btn-danger"
                  onClick={() => changeStatus(b.id, 2)}
                >
                  Reject
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
