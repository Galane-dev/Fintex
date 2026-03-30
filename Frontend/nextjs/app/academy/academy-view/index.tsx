"use client";

import { withAuth } from "@/hoc/withAuth";
import { AcademyContent } from "./academy-content";

const AcademyViewContent = () => {
  return <AcademyContent />;
};

export const AcademyView = withAuth(AcademyViewContent, {
  requireAcademyAccess: false,
});
