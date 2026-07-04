import axios from "axios";

export async function logout() {
  const refreshToken = localStorage.getItem("refreshToken");

  try {
    await axios.post("https://localhost:7142/api/auth/logout", {
      refreshToken,
    });
  } finally {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");

    window.location.href = "/login";
  }
}