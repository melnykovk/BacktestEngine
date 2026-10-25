import { deleteDbKlines, getKlineDatasets, uploadKlineDataset } from "../api/dbmanagerapi";
import { 
    type DbDatasetDeleteRequestDto, 
    type DbCandleSummaryResponseDto,
    type DbUploadKlinesRequestDto
} from "../api/dbmanagerdto";
import { useEffect, useState, useRef } from "react";



export interface ApiResult<T> {
    status: number;
    ok: boolean;
    body: T | string | null;
}

export function useDbManager() {
    const [datasets, setDatasets] = useState<DbCandleSummaryResponseDto[]>([]);
    const [datasetToDelete, setDatasetToDelete] = useState<DbDatasetDeleteRequestDto | null>(null);
        
    const [result, setResult] = useState<ApiResult<unknown> | null>(null);
    
    const dialogRef = useRef<HTMLDialogElement>(null);
    const abortRef = useRef<AbortController>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [isUploading, setIsUploading] = useState(false);
    const [isDeleting, setIsDeleting] = useState(false);
    const [deleteError, setDeleteError] = useState<string | null>(null);
    
    const [symbol, setSymbol] = useState("");
    const [interval, setInterval] = useState("4h");
    const [startDate, setStartDate] = useState("");
    const [endDate, setEndDate] = useState("");
    
    useEffect(() => {loadDatasets();}, []);
    
        function askDelete(dataset: DbDatasetDeleteRequestDto){
            setDatasetToDelete(dataset);
            setDeleteError(null);
            dialogRef.current?.showModal();
        };
    
        async function loadDatasets(){
            setIsLoading(true);
            try {
                const data = await getKlineDatasets();
                setDatasets(data);
            }
            finally{
                setIsLoading(false);
            }
        };
    
        async function confirmDelete(){
            if(!datasetToDelete) return
                    
            setIsDeleting(true);
            setDeleteError(null);
    
            try{
                const response = await deleteDbKlines({
                    symbol: datasetToDelete.symbol,
                    interval: datasetToDelete.interval
                });
                setResult(response);
                dialogRef.current?.close();
                loadDatasets();
            }catch(error){
                setDeleteError("Failed to delete");
            }finally {
                setIsDeleting(false);
            }
        };      
    
        async function uploadDataset(){
            const controller = new AbortController();
            abortRef.current = controller;
            setIsUploading(true);
            const payload: DbUploadKlinesRequestDto = {
                symbol,
                interval,
                startDate,
                endDate,
            };
            try{
                const response = await uploadKlineDataset(payload, controller.signal);
                setResult(response);
                setIsUploading(false);
                if(response.ok){
                await loadDatasets();
                }
            }catch (error){
                setIsUploading(false);
                const cancelled = error instanceof DOMException && error.name === "AbortError";
                setResult({
                    status: 0,
                    ok: false,
                    body: cancelled ? "Operation cancelled by user" : "Network error",
                });
            }finally {
                setIsUploading(false);
                abortRef.current = null
            }
            
        };
        function cancelUpload() {
            abortRef.current?.abort();
        }   
        return {
            datasets, datasetToDelete, result, dialogRef, isLoading, isUploading, isDeleting,
            deleteError, symbol, interval, startDate, endDate, askDelete, 
            confirmDelete, uploadDataset, setSymbol, setInterval, setStartDate,
            setEndDate, cancelUpload
        };
}