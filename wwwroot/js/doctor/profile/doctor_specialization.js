(function () {

    /*
     * =========================================================
     * SUB-SPECIALTY DATA
     * =========================================================
     */

    const subSpecialties = {

        "Anesthesiology": [
            "General Anesthesia",
            "Cardiac Anesthesia",
            "Neuroanesthesia",
            "Pediatric Anesthesia",
            "Obstetric Anesthesia",
            "Regional Anesthesia",
            "Critical Care Anesthesiology",
            "Ambulatory Anesthesia",
            "Pain Anesthesiology"
        ],

        "Cardiology": [
            "Interventional Cardiology",
            "Electrophysiology",
            "Heart Failure & Transplant Cardiology",
            "Preventive Cardiology",
            "Structural Heart Disease",
            "Adult Congenital Heart Disease",
            "Cardiac Imaging",
            "Nuclear Cardiology",
            "Sports Cardiology",
            "Cardio-Oncology"
        ],

        "Dermatology": [
            "Cosmetic Dermatology",
            "Pediatric Dermatology",
            "Dermatopathology",
            "Mohs Surgery",
            "Hair & Scalp Disorders",
            "Acne & Acne Scarring",
            "Psoriasis & Eczema",
            "Skin Cancer",
            "Contact Dermatitis",
            "Immunodermatology",
            "Laser & Aesthetic Dermatology",
            "Procedural Dermatology"
        ],

        "Emergency Medicine": [
            "Pediatric Emergency Medicine",
            "Emergency Critical Care",
            "Emergency Ultrasound",
            "Medical Toxicology",
            "Sports Emergency Medicine",
            "Disaster Medicine",
            "Wilderness Medicine",
            "Geriatric Emergency Medicine",
            "Emergency Psychiatry",
            "International Emergency Medicine"
        ],

        "Endocrinology": [
            "Diabetes",
            "Thyroid Disorders",
            "Adrenal Disorders",
            "Pituitary Disorders",
            "Metabolic Bone Disease",
            "Neuroendocrinology",
            "Reproductive Endocrinology",
            "Pediatric Endocrinology",
            "Obesity Medicine",
            "Endocrine Oncology"
        ],

        "Gastroenterology": [
            "Hepatology",
            "Interventional Gastroenterology",
            "Advanced Endoscopy",
            "Inflammatory Bowel Disease",
            "Gastrointestinal Motility",
            "Pancreatic & Biliary Disease",
            "Gastrointestinal Oncology",
            "Pediatric Gastroenterology",
            "Nutrition & Malabsorption",
            "Functional Gastrointestinal Disorders"
        ],

        "General Medicine": [
            "Internal Medicine",
            "Hospital Medicine",
            "Acute Medicine",
            "Ambulatory Medicine",
            "Geriatric Medicine",
            "Preventive Medicine",
            "Chronic Disease Management",
            "Complex Care Medicine"
        ],

        "General Physician": [
            "Primary Care",
            "Adult Medicine",
            "Preventive Care",
            "Chronic Disease Management",
            "Geriatric Care",
            "Men's Health",
            "Women's Health",
            "Lifestyle Medicine",
            "Health Screening"
        ],

        "Gynecology": [
            "General Gynecology",
            "Obstetrics & Gynecology",
            "Gynecologic Oncology",
            "Urogynecology",
            "Reproductive Endocrinology",
            "Maternal-Fetal Medicine",
            "Minimally Invasive Gynecologic Surgery",
            "Pediatric & Adolescent Gynecology",
            "Menopause Medicine",
            "High-Risk Pregnancy"
        ],

        "Hematology": [
            "Benign Hematology",
            "Hematologic Malignancies",
            "Hemostasis & Thrombosis",
            "Transfusion Medicine",
            "Pediatric Hematology",
            "Bone Marrow Transplantation",
            "Cellular Therapy",
            "Hematologic Pathology",
            "Red Blood Cell Disorders",
            "Platelet Disorders"
        ],

        "Nephrology": [
            "Interventional Nephrology",
            "Transplant Nephrology",
            "Pediatric Nephrology",
            "Dialysis Medicine",
            "Glomerular Diseases",
            "Hypertension",
            "Critical Care Nephrology",
            "Kidney Stone Disease",
            "Electrolyte Disorders",
            "Kidney Disease in Pregnancy"
        ],

        "Neurology": [
            "Epilepsy",
            "Stroke & Cerebrovascular Disease",
            "Movement Disorders",
            "Multiple Sclerosis & Neuroimmunology",
            "Neuromuscular Medicine",
            "Neurocritical Care",
            "Behavioral Neurology",
            "Headache Medicine",
            "Sleep Neurology",
            "Neuro-oncology",
            "Autonomic Neurology",
            "Vascular Neurology"
        ],

        "Oncology": [
            "Breast Oncology",
            "Lung Oncology",
            "Gastrointestinal Oncology",
            "Genitourinary Oncology",
            "Gynecologic Oncology",
            "Head & Neck Oncology",
            "Neuro-oncology",
            "Pediatric Oncology",
            "Sarcoma Oncology",
            "Melanoma & Skin Cancer",
            "Precision Oncology",
            "Geriatric Oncology"
        ],

        "Ophthalmology": [
            "Cornea & External Disease",
            "Retina & Vitreous",
            "Glaucoma",
            "Pediatric Ophthalmology",
            "Neuro-Ophthalmology",
            "Oculoplastic Surgery",
            "Cataract Surgery",
            "Uveitis",
            "Ocular Oncology",
            "Refractive Surgery",
            "Anterior Segment Surgery",
            "Vitreoretinal Surgery"
        ],

        "Orthopedics": [
            "Joint Replacement",
            "Sports Orthopedics",
            "Spine Surgery",
            "Hand & Upper Extremity",
            "Foot & Ankle",
            "Pediatric Orthopedics",
            "Orthopedic Trauma",
            "Musculoskeletal Oncology",
            "Shoulder & Elbow",
            "Hip & Knee",
            "Orthopedic Oncology",
            "Orthopedic Reconstruction"
        ],

        "Pediatrics": [
            "Neonatology",
            "Pediatric Cardiology",
            "Pediatric Neurology",
            "Pediatric Gastroenterology",
            "Pediatric Pulmonology",
            "Pediatric Endocrinology",
            "Pediatric Nephrology",
            "Pediatric Hematology & Oncology",
            "Pediatric Infectious Disease",
            "Pediatric Emergency Medicine",
            "Pediatric Allergy & Immunology",
            "Developmental-Behavioral Pediatrics",
            "Adolescent Medicine",
            "Pediatric Critical Care",
            "Pediatric Rheumatology"
        ],

        "Psychiatry": [
            "Child & Adolescent Psychiatry",
            "Geriatric Psychiatry",
            "Addiction Psychiatry",
            "Consultation-Liaison Psychiatry",
            "Forensic Psychiatry",
            "Emergency Psychiatry",
            "Neuropsychiatry",
            "Community Psychiatry",
            "Women's Mental Health",
            "Psychosomatic Medicine",
            "Sleep Psychiatry"
        ],

        "Pulmonology": [
            "Critical Care Pulmonology",
            "Interventional Pulmonology",
            "Sleep Medicine",
            "Pediatric Pulmonology",
            "Pulmonary Hypertension",
            "Interstitial Lung Disease",
            "COPD",
            "Asthma",
            "Lung Transplantation",
            "Occupational Lung Disease",
            "Pulmonary Oncology",
            "Respiratory Failure"
        ],

        "Radiology": [
            "Diagnostic Radiology",
            "Interventional Radiology",
            "Neuroradiology",
            "Musculoskeletal Radiology",
            "Breast Imaging",
            "Cardiothoracic Radiology",
            "Abdominal Radiology",
            "Pediatric Radiology",
            "Emergency Radiology",
            "Nuclear Radiology",
            "Molecular Imaging",
            "Head & Neck Radiology"
        ],

        "Urology": [
            "Urologic Oncology",
            "Pediatric Urology",
            "Female Pelvic Medicine",
            "Male Infertility",
            "Andrology",
            "Endourology",
            "Stone Disease",
            "Reconstructive Urology",
            "Neuro-Urology",
            "Transplant Urology",
            "Robotic Urologic Surgery",
            "Female Urology"
        ],

        "ENT": [
            "Otology",
            "Neurotology",
            "Rhinology & Sinus Surgery",
            "Head & Neck Surgery",
            "Pediatric Otolaryngology",
            "Laryngology",
            "Facial Plastic Surgery",
            "Sleep Surgery",
            "Skull Base Surgery",
            "Otolaryngic Oncology",
            "Voice & Swallowing Disorders"
        ],

        "Allergy & Immunology": [
            "Pediatric Allergy",
            "Adult Allergy",
            "Food Allergy",
            "Drug Allergy",
            "Asthma & Allergic Lung Disease",
            "Immunodeficiency",
            "Autoimmune Disease",
            "Atopic Dermatitis",
            "Immunotherapy",
            "Environmental Allergy",
            "Occupational Allergy"
        ],

        "Rheumatology": [
            "Adult Rheumatology",
            "Pediatric Rheumatology",
            "Rheumatoid Arthritis",
            "Lupus",
            "Vasculitis",
            "Spondyloarthritis",
            "Gout & Crystal Arthritis",
            "Osteoarthritis",
            "Osteoporosis",
            "Connective Tissue Diseases",
            "Autoimmune Diseases"
        ],

        "Infectious Disease": [
            "General Infectious Disease",
            "HIV Medicine",
            "Tropical Medicine",
            "Travel Medicine",
            "Antimicrobial Stewardship",
            "Hospital-Acquired Infections",
            "Pediatric Infectious Disease",
            "Transplant Infectious Disease",
            "Infection Prevention & Control",
            "Tuberculosis",
            "Viral Infections",
            "Bacterial Infections"
        ],

        "Geriatrics": [
            "Geriatric Medicine",
            "Dementia & Cognitive Disorders",
            "Falls & Mobility",
            "Frailty Medicine",
            "Palliative Care",
            "Long-Term Care Medicine",
            "Geriatric Psychiatry",
            "Geriatric Rehabilitation",
            "Polypharmacy Management",
            "Healthy Aging"
        ],

        "Sports Medicine": [
            "Orthopedic Sports Medicine",
            "Primary Care Sports Medicine",
            "Exercise Medicine",
            "Sports Injury Rehabilitation",
            "Musculoskeletal Medicine",
            "Concussion Medicine",
            "Sports Cardiology",
            "Sports Nutrition",
            "Sports Psychology",
            "Performance Medicine"
        ],

        "Nutrition": [
            "Clinical Nutrition",
            "Pediatric Nutrition",
            "Sports Nutrition",
            "Obesity & Weight Management",
            "Renal Nutrition",
            "Oncology Nutrition",
            "Gastrointestinal Nutrition",
            "Diabetes Nutrition",
            "Critical Care Nutrition",
            "Enteral & Parenteral Nutrition"
        ],

        "Pain Management": [
            "Interventional Pain Medicine",
            "Chronic Pain",
            "Cancer Pain",
            "Neuropathic Pain",
            "Musculoskeletal Pain",
            "Pediatric Pain",
            "Acute Pain",
            "Pain Rehabilitation",
            "Spine Pain",
            "Headache & Facial Pain"
        ],

        "Sleep Medicine": [
            "Sleep Apnea",
            "Insomnia",
            "Narcolepsy",
            "Circadian Rhythm Disorders",
            "Parasomnias",
            "Sleep-Related Movement Disorders",
            "Pediatric Sleep Medicine",
            "Sleep Breathing Disorders",
            "Behavioral Sleep Medicine",
            "Sleep Neurology"
        ],

        "Occupational Medicine": [
            "Workplace Injury",
            "Occupational Toxicology",
            "Industrial Medicine",
            "Occupational Ergonomics",
            "Workplace Health",
            "Disability Evaluation",
            "Occupational Rehabilitation",
            "Environmental Medicine",
            "Occupational Respiratory Medicine",
            "Travel & Occupational Health"
        ],

        "Plastic Surgery": [
            "Cosmetic Surgery",
            "Reconstructive Surgery",
            "Craniofacial Surgery",
            "Hand Surgery",
            "Microsurgery",
            "Burn Surgery",
            "Breast Reconstruction",
            "Aesthetic Surgery",
            "Pediatric Plastic Surgery",
            "Facial Plastic Surgery",
            "Gender-Affirming Surgery"
        ],

        "Psychology": [
            "Clinical Psychology",
            "Counseling Psychology",
            "Neuropsychology",
            "Child Psychology",
            "Adolescent Psychology",
            "Health Psychology",
            "Rehabilitation Psychology",
            "Forensic Psychology",
            "Behavioral Psychology",
            "Sports Psychology",
            "Addiction Psychology",
            "Industrial & Organizational Psychology"
        ],

        "Neurosurgery": [
            "Brain Tumor Surgery",
            "Spine Surgery",
            "Cerebrovascular Neurosurgery",
            "Endovascular Neurosurgery",
            "Pediatric Neurosurgery",
            "Functional Neurosurgery",
            "Epilepsy Surgery",
            "Skull Base Surgery",
            "Neurotrauma",
            "Peripheral Nerve Surgery",
            "Neurovascular Surgery",
            "Stereotactic Neurosurgery"
        ],

        "Cardiothoracic Surgery": [
            "Cardiac Surgery",
            "Coronary Artery Bypass Surgery",
            "Valve Surgery",
            "Aortic Surgery",
            "Congenital Heart Surgery",
            "Thoracic Surgery",
            "Lung Surgery",
            "Esophageal Surgery",
            "Heart Transplantation",
            "Mechanical Circulatory Support",
            "Minimally Invasive Cardiac Surgery"
        ],

        "Vascular Surgery": [
            "Arterial Surgery",
            "Aortic Surgery",
            "Carotid Surgery",
            "Peripheral Arterial Disease",
            "Venous Surgery",
            "Varicose Vein Treatment",
            "Endovascular Surgery",
            "Dialysis Access Surgery",
            "Limb Salvage",
            "Diabetic Vascular Disease",
            "Vascular Trauma"
        ],

        "General Surgery": [
            "Acute Care Surgery",
            "Trauma Surgery",
            "Breast Surgery",
            "Endocrine Surgery",
            "Hepatobiliary Surgery",
            "Upper GI Surgery",
            "Bariatric Surgery",
            "Minimally Invasive Surgery",
            "Transplant Surgery",
            "Surgical Oncology",
            "Hernia Surgery",
            "Robotic Surgery"
        ],

        "Colorectal Surgery": [
            "Colon Surgery",
            "Rectal Surgery",
            "Anal Surgery",
            "Inflammatory Bowel Disease Surgery",
            "Colorectal Cancer Surgery",
            "Pelvic Floor Disorders",
            "Proctology",
            "Minimally Invasive Colorectal Surgery",
            "Colorectal Trauma",
            "Complex Abdominal Surgery"
        ],

        "Oral & Maxillofacial Surgery": [
            "Oral Surgery",
            "Maxillofacial Trauma",
            "Orthognathic Surgery",
            "Facial Reconstruction",
            "Dental Implant Surgery",
            "Head & Neck Surgery",
            "Cleft & Craniofacial Surgery",
            "TMJ Surgery",
            "Oral Cancer Surgery",
            "Facial Cosmetic Surgery",
            "Dentoalveolar Surgery"
        ],

        "Pediatric Surgery": [
            "Neonatal Surgery",
            "Pediatric General Surgery",
            "Pediatric Thoracic Surgery",
            "Pediatric Urology",
            "Pediatric Oncology Surgery",
            "Pediatric Trauma Surgery",
            "Pediatric Minimally Invasive Surgery",
            "Pediatric Hepatobiliary Surgery",
            "Pediatric Colorectal Surgery",
            "Pediatric Vascular Surgery"
        ],

        "Reconstructive Surgery": [
            "Microsurgery",
            "Breast Reconstruction",
            "Head & Neck Reconstruction",
            "Limb Reconstruction",
            "Hand Reconstruction",
            "Craniofacial Reconstruction",
            "Burn Reconstruction",
            "Maxillofacial Reconstruction",
            "Nerve Reconstruction",
            "Complex Wound Reconstruction",
            "Scar Revision"
        ],

        "Maternal-Fetal Medicine": [
            "High-Risk Pregnancy",
            "Maternal Medical Disorders",
            "Fetal Medicine",
            "Prenatal Diagnosis",
            "Fetal Ultrasound",
            "Fetal Therapy",
            "Multiple Pregnancy",
            "Obstetric Critical Care",
            "Fetal Genetics",
            "Maternal Cardiovascular Disease",
            "Maternal Diabetes"
        ],

        "Reproductive Endocrinology": [
            "Infertility",
            "IVF",
            "Fertility Preservation",
            "Reproductive Hormonal Disorders",
            "Polycystic Ovary Syndrome",
            "Endometriosis",
            "Recurrent Pregnancy Loss",
            "Menopause & Reproductive Aging",
            "Ovulation Disorders",
            "Reproductive Genetics"
        ],

        "Fertility & Reproductive Medicine": [
            "IVF",
            "IUI",
            "Fertility Preservation",
            "Egg Freezing",
            "Male Infertility",
            "Female Infertility",
            "Donor Egg Programs",
            "Donor Sperm Programs",
            "Embryology",
            "Reproductive Genetics",
            "Fertility Counseling",
            "Assisted Reproductive Technology"
        ],

        "Neonatology": [
            "Neonatal Intensive Care",
            "Prematurity",
            "Neonatal Respiratory Care",
            "Neonatal Cardiology",
            "Neonatal Neurology",
            "Neonatal Surgery",
            "Neonatal Infectious Disease",
            "Neonatal Nutrition",
            "Neonatal Hematology",
            "Neonatal Genetics",
            "Neonatal Transport Medicine"
        ],

        "Pediatric Cardiology": [
            "Pediatric Electrophysiology",
            "Pediatric Interventional Cardiology",
            "Pediatric Heart Failure",
            "Congenital Heart Disease",
            "Fetal Cardiology",
            "Pediatric Cardiac Imaging",
            "Pediatric Cardiac Intensive Care",
            "Pediatric Preventive Cardiology",
            "Pediatric Pulmonary Hypertension",
            "Pediatric Cardiac Surgery"
        ],

        "Pediatric Neurology": [
            "Pediatric Epilepsy",
            "Pediatric Movement Disorders",
            "Neuromuscular Disorders",
            "Neurodevelopmental Disorders",
            "Pediatric Neuroimmunology",
            "Pediatric Stroke",
            "Pediatric Headache",
            "Neurometabolic Disorders",
            "Pediatric Neuro-oncology",
            "Pediatric Sleep Neurology"
        ],

        "Pediatric Gastroenterology": [
            "Pediatric Hepatology",
            "Pediatric IBD",
            "Pediatric Nutrition",
            "Pediatric Motility",
            "Pediatric Pancreatic Disease",
            "Pediatric Endoscopy",
            "Pediatric Celiac Disease",
            "Pediatric GI Oncology",
            "Pediatric Feeding Disorders"
        ],

        "Pediatric Pulmonology": [
            "Pediatric Asthma",
            "Cystic Fibrosis",
            "Pediatric Sleep Medicine",
            "Pediatric Interstitial Lung Disease",
            "Pediatric Pulmonary Hypertension",
            "Pediatric Respiratory Disorders",
            "Pediatric Bronchoscopy",
            "Pediatric Chronic Lung Disease",
            "Pediatric Respiratory Failure"
        ],

        "Interventional Cardiology": [
            "Coronary Intervention",
            "Structural Heart Intervention",
            "Peripheral Intervention",
            "Complex PCI",
            "Chronic Total Occlusion (CTO)",
            "Interventional Imaging",
            "Transcatheter Aortic Valve Replacement (TAVR)",
            "Mitral Valve Intervention",
            "Left Atrial Appendage Closure",
            "Congenital Heart Intervention"
        ],

        "Interventional Radiology": [
            "Vascular Intervention",
            "Interventional Oncology",
            "Neurointervention",
            "Embolization",
            "Image-Guided Biopsy",
            "Drainage Procedures",
            "Interventional Pain",
            "Women's Health Interventions",
            "Dialysis Access Intervention",
            "Hepatobiliary Intervention",
            "Uterine Fibroid Embolization"
        ],

        "Radiation Oncology": [
            "Breast Radiation",
            "Prostate Radiation",
            "Lung Radiation",
            "Brain & Spine Radiation",
            "Head & Neck Radiation",
            "GI Radiation",
            "Gynecologic Radiation",
            "Pediatric Radiation",
            "Stereotactic Radiosurgery",
            "Proton Therapy",
            "Brachytherapy",
            "Stereotactic Body Radiation Therapy",
            "Image-Guided Radiation Therapy"
        ],

        "Medical Oncology": [
            "Breast Oncology",
            "Lung Oncology",
            "Gastrointestinal Oncology",
            "Genitourinary Oncology",
            "Gynecologic Oncology",
            "Head & Neck Oncology",
            "Neuro-oncology",
            "Sarcoma Oncology",
            "Melanoma & Skin Cancer",
            "Precision Oncology",
            "Immunotherapy",
            "Targeted Therapy"
        ],

        "Surgical Oncology": [
            "Breast Surgical Oncology",
            "GI Surgical Oncology",
            "Hepatobiliary Surgery",
            "Pancreatic Surgery",
            "Endocrine Oncology",
            "Melanoma Surgery",
            "Sarcoma Surgery",
            "Peritoneal Surface Malignancy Surgery",
            "Thoracic Surgical Oncology",
            "Gynecologic Surgical Oncology",
            "Head & Neck Surgical Oncology"
        ],

        "Hematology & Oncology": [
            "Leukemia",
            "Lymphoma",
            "Multiple Myeloma",
            "Myelodysplastic Syndromes",
            "Myeloproliferative Neoplasms",
            "Solid Tumor Oncology",
            "Pediatric Hematology/Oncology",
            "Bone Marrow Transplantation",
            "Cellular Therapy",
            "Hematologic Malignancies",
            "Cancer Immunotherapy"
        ],

        "Clinical Pathology": [
            "Hematopathology",
            "Clinical Chemistry",
            "Microbiology",
            "Immunology",
            "Transfusion Medicine",
            "Molecular Pathology",
            "Cytogenetics",
            "Clinical Immunopathology",
            "Toxicology",
            "Laboratory Medicine",
            "Genetic Pathology"
        ],

        "Anatomical Pathology": [
            "Surgical Pathology",
            "Cytopathology",
            "Dermatopathology",
            "Hematopathology",
            "Neuropathology",
            "Renal Pathology",
            "Breast Pathology",
            "GI Pathology",
            "Gynecologic Pathology",
            "Pediatric Pathology",
            "Pulmonary Pathology",
            "Bone & Soft Tissue Pathology",
            "Urologic Pathology",
            "Head & Neck Pathology"
        ],

        "Nuclear Medicine": [
            "Nuclear Cardiology",
            "PET/CT",
            "Molecular Imaging",
            "Nuclear Oncology",
            "Thyroid Nuclear Medicine",
            "Theranostics",
            "Radionuclide Therapy",
            "Pediatric Nuclear Medicine",
            "Neuro-Nuclear Medicine",
            "Nuclear Gastroenterology",
            "Nuclear Endocrinology"
        ],

        "Physical Medicine & Rehabilitation": [
            "Brain Injury Rehabilitation",
            "Spinal Cord Injury",
            "Sports Rehabilitation",
            "Musculoskeletal Rehabilitation",
            "Pain Rehabilitation",
            "Pediatric Rehabilitation",
            "Neuromuscular Rehabilitation",
            "Stroke Rehabilitation",
            "Amputee Rehabilitation",
            "Cancer Rehabilitation",
            "Cardiac Rehabilitation",
            "Pulmonary Rehabilitation"
        ],

        "Family Medicine": [
            "Family Primary Care",
            "Sports Medicine",
            "Geriatric Medicine",
            "Adolescent Medicine",
            "Women's Health",
            "Men's Health",
            "Preventive Medicine",
            "Rural Medicine",
            "Addiction Medicine",
            "Palliative Care",
            "Maternal & Child Health",
            "Behavioral Health"
        ],

        "Preventive Medicine": [
            "Public Health",
            "Occupational Medicine",
            "Aerospace Medicine",
            "Lifestyle Medicine",
            "Environmental Medicine",
            "Clinical Preventive Medicine",
            "Epidemiology",
            "Health Administration",
            "Population Health",
            "Preventive Cardiology",
            "Preventive Oncology",
            "Disease Prevention & Screening"
        ]
    };


    /*
     * =========================================================
     * INITIALIZE SPECIALIZATION
     * =========================================================
     */

    function initializeSpecialization() {

        const specializationSelect =
            document.getElementById("specializationSelect");

        const subSpecialtySelect =
            document.getElementById("subSpecialtySelect");

        if (!specializationSelect || !subSpecialtySelect) {
            return;
        }


        /*
         * Prevent attaching the change event multiple times
         * when HTMX loads the partial more than once.
         */

        if (specializationSelect.dataset.specializationInitialized === "true") {
            return;
        }

        specializationSelect.dataset.specializationInitialized = "true";


        /*
         * Existing saved sub-specialty
         */

        const selectedSubSpecialty =
            subSpecialtySelect.getAttribute("data-selected") || "";


        /*
         * Populate sub-specialties
         */

        function populateSubSpecialties(selectedValue) {

            const specialization =
                specializationSelect.value;


            /*
             * Clear existing options
             */

            subSpecialtySelect.innerHTML = "";


            /*
             * No specialization selected
             */

            if (!specialization) {

                subSpecialtySelect.disabled = true;

                const option =
                    document.createElement("option");

                option.value = "";
                option.textContent =
                    "Select a specialization first";

                subSpecialtySelect.appendChild(option);

                return;
            }


            /*
             * Specialization exists but has no data
             */

            if (!subSpecialties[specialization]) {

                console.warn(
                    "No sub-specialties found for:",
                    specialization
                );

                subSpecialtySelect.disabled = true;

                const option =
                    document.createElement("option");

                option.value = "";
                option.textContent =
                    "No sub-specialties available";

                subSpecialtySelect.appendChild(option);

                return;
            }


            /*
             * Enable dropdown
             */

            subSpecialtySelect.disabled = false;


            /*
             * Default option
             */

            const defaultOption =
                document.createElement("option");

            defaultOption.value = "";
            defaultOption.textContent =
                "Select sub-specialty";

            subSpecialtySelect.appendChild(defaultOption);


            /*
             * Add sub-specialties
             */

            subSpecialties[specialization].forEach(function (subSpecialty) {

                const option =
                    document.createElement("option");

                option.value = subSpecialty;
                option.textContent = subSpecialty;

                if (subSpecialty === selectedValue) {
                    option.selected = true;
                }

                subSpecialtySelect.appendChild(option);

            });


            /*
             * If the saved value doesn't exist,
             * keep the default option selected.
             */

            if (
                selectedValue &&
                !subSpecialties[specialization].includes(selectedValue)
            ) {
                console.warn(
                    "Saved sub-specialty does not belong to specialization:",
                    selectedValue,
                    specialization
                );

                subSpecialtySelect.value = "";
            }
        }


        /*
         * =====================================================
         * SPECIALIZATION CHANGE
         * =====================================================
         */

        specializationSelect.addEventListener(
            "change",
            function () {

                populateSubSpecialties("");

            }
        );


        /*
         * =====================================================
         * INITIAL LOAD
         * =====================================================
         *
         * This is important for Edit/Profile pages.
         *
         * Example:
         *
         * Specialization = General Medicine
         * SubSpecialties = Hospital Medicine
         *
         * The sub-specialty dropdown will automatically
         * populate and select Hospital Medicine.
         */

        populateSubSpecialties(selectedSubSpecialty);


        /*
         * =====================================================
         * SERVICES CHARACTER COUNTER
         * =====================================================
         */

        const servicesInput =
            document.querySelector(
                '[name="ServicesOffered"]'
            );

        const counter =
            document.querySelector(
                '[data-for="ServicesOffered"]'
            );


        if (servicesInput && counter) {

            function updateCounter() {

                counter.textContent =
                    `${servicesInput.value.length} / 50`;

            }

            servicesInput.addEventListener(
                "input",
                updateCounter
            );

            updateCounter();
        }

    }


    /*
     * =========================================================
     * NORMAL PAGE LOAD
     * =========================================================
     *
     * If this script is loaded normally before DOMContentLoaded,
     * wait for DOMContentLoaded.
     *
     * If it is loaded through HTMX after DOMContentLoaded,
     * initialize immediately.
     */

    if (document.readyState === "loading") {

        document.addEventListener(
            "DOMContentLoaded",
            initializeSpecialization
        );

    } else {

        initializeSpecialization();

    }


    /*
     * =========================================================
     * HTMX SUPPORT
     * =========================================================
     *
     * Your partial is being loaded dynamically.
     *
     * This makes the script work when the partial is inserted
     * by HTMX after the main page has already loaded.
     */

    document.body.addEventListener(
        "htmx:afterSwap",
        function () {

            initializeSpecialization();

        }
    );


    /*
     * =========================================================
     * HTMX LOAD SUPPORT
     * =========================================================
     */

    document.body.addEventListener(
        "htmx:afterSettle",
        function () {

            initializeSpecialization();

        }
    );

})();
