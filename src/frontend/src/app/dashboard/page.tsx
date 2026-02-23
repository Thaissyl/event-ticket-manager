import { auth, signOut } from "@/lib/auth";

export default async function DashboardPage() {
  const session = await auth();

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              <h1 className="text-xl font-bold text-gray-900">Event Tickets</h1>
            </div>
            <div className="flex items-center gap-4">
              <span className="text-sm text-gray-700">{session?.user?.email}</span>
              <form
                action={async () => {
                  "use server";
                  await signOut();
                }}
              >
                <button
                  type="submit"
                  className="text-sm text-gray-600 hover:text-gray-900"
                >
                  Sign Out
                </button>
              </form>
            </div>
          </div>
        </div>
      </nav>

      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="bg-white shadow rounded-lg p-6">
          <h2 className="text-2xl font-bold text-gray-900 mb-4">Welcome to Dashboard</h2>
          <p className="text-gray-600 mb-4">
            You are logged in as <strong>{session?.user?.email}</strong>
          </p>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mt-8">
            <div className="bg-blue-50 p-6 rounded-lg">
              <h3 className="font-semibold text-blue-900 mb-2">My Events</h3>
              <p className="text-3xl font-bold text-blue-600">0</p>
            </div>
            <div className="bg-green-50 p-6 rounded-lg">
              <h3 className="font-semibold text-green-900 mb-2">Tickets Sold</h3>
              <p className="text-3xl font-bold text-green-600">0</p>
            </div>
            <div className="bg-purple-50 p-6 rounded-lg">
              <h3 className="font-semibold text-purple-900 mb-2">Revenue</h3>
              <p className="text-3xl font-bold text-purple-600">$0</p>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
}
