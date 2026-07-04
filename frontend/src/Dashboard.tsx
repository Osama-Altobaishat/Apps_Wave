import ResourcesPage from "./ResourcesPage";
import AdminBooking from "./AdminBooking";
import UsersInfo from "./UsersInfo";

export default function Dashboard() {
  return (
    <div>
      <ResourcesPage />
      <AdminBooking />
      <UsersInfo />
    </div>
  );
}
