/** @typedef {import("../api/createTaxCategoryTypes.js").CreateTaxCategoryRequest} CreateTaxCategoryRequest */
/** @typedef {import("../api/createTaxCategoryTypes.js").CreateTaxCategoryResponse} CreateTaxCategoryResponse */

import { zodResolver } from "@hookform/resolvers/zod";
import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  MenuItem,
  Stack,
  TextField,
} from "@mui/material";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { ApiError } from "../../../shared/api/ApiError";

const codePattern = /^[A-Z0-9-]+$/;
const percentagePattern = /^\d+(\.\d{1,2})?$/;

const createTaxCategorySchema = z
  .object({
    code: z
      .string()
      .trim()
      .min(1, "Tax category code is required.")
      .max(30, "Tax category code must not exceed 30 characters.")
      .transform((value) => value.toUpperCase())
      .refine(
        (value) => codePattern.test(value),
        "Tax category code can only contain letters, numbers, and hyphens.",
      ),
    name: z
      .string()
      .trim()
      .min(1, "Tax category name is required.")
      .max(100, "Tax category name must not exceed 100 characters.")
      .transform((value) => value.replace(/\s+/g, " ")),
    description: z
      .string()
      .trim()
      .max(500, "Tax category description must not exceed 500 characters."),
    ratePercent: z
      .string()
      .trim()
      .min(1, "Tax rate is required.")
      .refine(
        (value) => percentagePattern.test(value),
        "Tax rate must use no more than 2 decimal places.",
      )
      .transform(Number)
      .refine(
        (value) => value >= 0 && value <= 100,
        "Tax rate must be between 0 and 100 percent.",
      ),
    treatment: z.enum(["StandardRated", "ZeroRated", "Exempt"]),
    effectiveFrom: z.string().min(1, "Effective-from date is required."),
    effectiveTo: z.string(),
  })
  .superRefine((values, context) => {
    const rateMatchesTreatment =
      values.treatment === "StandardRated"
        ? values.ratePercent > 0
        : values.ratePercent === 0;

    if (!rateMatchesTreatment) {
      context.addIssue({
        code: "custom",
        path: ["ratePercent"],
        message:
          values.treatment === "StandardRated"
            ? "A standard-rated category must have a rate above 0%."
            : "A zero-rated or exempt category must have a 0% rate.",
      });
    }

    if (
      values.effectiveTo !== "" &&
      values.effectiveTo < values.effectiveFrom
    ) {
      context.addIssue({
        code: "custom",
        path: ["effectiveTo"],
        message:
          "Effective-to date cannot be earlier than effective-from date.",
      });
    }
  });

const emptyFormValues = {
  code: "",
  name: "",
  description: "",
  ratePercent: "",
  treatment: "StandardRated",
  effectiveFrom: "",
  effectiveTo: "",
};

/**
 * @param {{
 *   open: boolean,
 *   isPending: boolean,
 *   error: unknown,
 *   onClose: () => void,
 *   onSubmit: (request: CreateTaxCategoryRequest) => Promise<CreateTaxCategoryResponse>
 * }} props
 */
export function CreateTaxCategoryDialog({
  open,
  isPending,
  error,
  onClose,
  onSubmit,
}) {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(createTaxCategorySchema),
    defaultValues: emptyFormValues,
  });

  useEffect(() => {
    if (open) {
      reset(emptyFormValues);
    }
  }, [open, reset]);

  async function handleValidSubmit(values) {
    try {
      const createdCategory = await onSubmit({
        code: values.code,
        name: values.name,
        description: values.description === "" ? null : values.description,
        rate: values.ratePercent / 100,
        treatment: values.treatment,
        effectiveFrom: values.effectiveFrom,
        effectiveTo: values.effectiveTo === "" ? null : values.effectiveTo,
      });
      onClose();
      return createdCategory;
    } catch {
      return undefined;
    }
  }

  function handleDialogClose() {
    if (!isPending) {
      onClose();
    }
  }

  return (
    <Dialog
      open={open}
      onClose={handleDialogClose}
      fullWidth
      maxWidth="sm"
      aria-labelledby="create-tax-category-dialog-title"
    >
      <Box
        component="form"
        noValidate
        onSubmit={handleSubmit(handleValidSubmit)}
      >
        <DialogTitle id="create-tax-category-dialog-title">
          Create tax category
        </DialogTitle>

        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label="Tax category code"
              required
              autoFocus
              disabled={isPending}
              error={Boolean(errors.code)}
              helperText={errors.code?.message}
              {...register("code")}
            />
            <TextField
              label="Tax category name"
              required
              disabled={isPending}
              error={Boolean(errors.name)}
              helperText={errors.name?.message}
              {...register("name")}
            />
            <TextField
              label="Description"
              multiline
              minRows={3}
              disabled={isPending}
              error={Boolean(errors.description)}
              helperText={errors.description?.message}
              {...register("description")}
            />
            <TextField
              label="Treatment"
              select
              required
              disabled={isPending}
              defaultValue="StandardRated"
              {...register("treatment")}
            >
              <MenuItem value="StandardRated">Standard rated</MenuItem>
              <MenuItem value="ZeroRated">Zero rated</MenuItem>
              <MenuItem value="Exempt">Exempt</MenuItem>
            </TextField>
            <TextField
              label="Rate (%)"
              required
              inputMode="decimal"
              disabled={isPending}
              error={Boolean(errors.ratePercent)}
              helperText={errors.ratePercent?.message}
              {...register("ratePercent")}
            />
            <TextField
              label="Effective from"
              type="date"
              required
              disabled={isPending}
              error={Boolean(errors.effectiveFrom)}
              helperText={errors.effectiveFrom?.message}
              slotProps={{ inputLabel: { shrink: true } }}
              {...register("effectiveFrom")}
            />
            <TextField
              label="Effective to"
              type="date"
              disabled={isPending}
              error={Boolean(errors.effectiveTo)}
              helperText={errors.effectiveTo?.message ?? "Optional"}
              slotProps={{ inputLabel: { shrink: true } }}
              {...register("effectiveTo")}
            />

            {error && <Alert severity="error">{getErrorMessage(error)}</Alert>}
          </Stack>
        </DialogContent>

        <DialogActions>
          <Button
            type="button"
            disabled={isPending}
            onClick={handleDialogClose}
          >
            Cancel
          </Button>
          <Button type="submit" variant="contained" disabled={isPending}>
            {isPending ? "Creating..." : "Create tax category"}
          </Button>
        </DialogActions>
      </Box>
    </Dialog>
  );
}

function getErrorMessage(error) {
  if (error instanceof ApiError) {
    return error.detail;
  }

  return "Unable to create the tax category.";
}
