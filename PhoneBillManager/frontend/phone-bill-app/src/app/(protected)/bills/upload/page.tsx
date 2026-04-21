import TopBar from "@/components/layout/TopBar";
import BillUpload from "@/components/bills/BillUpload";

export const metadata = { title: "Upload Bill" };

export default function UploadPage() {
  return (
    <>
      <TopBar title="Upload Bill" showBack />
      <div className="px-4 py-6">
        <p className="text-sm text-gray-500 mb-6">
          Upload your carrier-generated PDF bill. We&apos;ll automatically extract Plans, Equipment, and Services charges per line.
        </p>
        <BillUpload />
      </div>
    </>
  );
}
