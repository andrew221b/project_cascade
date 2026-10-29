import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Flying Ladders · Support & privacy",
  description: "Game support, purchases, privacy and account deletion for Flying Ladders.",
  icons: {
    icon: "/favicon.svg",
    shortcut: "/favicon.svg",
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className="antialiased">{children}</body>
    </html>
  );
}
