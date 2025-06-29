import { useCallback, useState } from 'react';
import { Controller, SubmitHandler, useForm } from 'react-hook-form';
import LibraryType from 'src/core/models/LibraryType';
import useLibraries from 'src/renderer/hooks/useLibraries';
import { Button } from '../ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../ui/dialog';
import { Input } from '../ui/input';
import { Label } from '../ui/label';
import { RadioGroup, RadioGroupItem } from '../ui/radio-group';

type LibraryCreationForm = {
  path: string;
  label: string;
  platform: LibraryType;
};

const LibraryCreationDialog = () => {
  const [showLabelDialog, setShowLabelDialog] = useState(false);
  const { create, refreshLibraries } = useLibraries();
  const {
    register,
    handleSubmit,
    watch,
    control,
    formState: { errors },
    setValue,
    reset,
  } = useForm<LibraryCreationForm>({
    defaultValues: {
      platform: 'slm',
    },
  });

  const resetDialog = useCallback(() => {
    setShowLabelDialog(false);
    reset();
  }, []);

  const onSubmit: SubmitHandler<LibraryCreationForm> = async (libraryData) => {
    try {
      await create(libraryData.path, libraryData.label, libraryData.platform);

      resetDialog();
    } catch (err) {
      alert(err);
    }
  };

  const openLibraryDirectorySelector = async () => {
    const path = await window.api['select-directory']();

    if (!path) {
      return;
    }

    setValue('path', path);
    setShowLabelDialog(true);
  };

  return (
    <>
      <button className='text-blue-600 hover:underline text-left' onClick={() => openLibraryDirectorySelector()}>
        + Kütüphane Oluştur
      </button>
      {showLabelDialog && (
        <Dialog open onOpenChange={resetDialog}>
          <DialogContent className='bg-white'>
            <DialogHeader>
              <DialogTitle>Yeni Kütüphane Oluştur</DialogTitle>
            </DialogHeader>
            <form onSubmit={handleSubmit(onSubmit)}>
              <div className='mb-2 select-none'>
                📁 {watch('path')} <span onClick={() => openLibraryDirectorySelector()}>🔃</span>
              </div>
              <label htmlFor='label' className='text-sm font-medium'>
                Etiket (İsteğe Bağlı)
              </label>
              <Input id='label' placeholder='Örn: SSD Oyunlar, Arşiv, Retro...' {...register('label')} autoFocus />
              <label htmlFor='label' className='text-sm font-medium'>
                Platform
              </label>
              <Controller
                name='platform'
                control={control}
                rules={{ required: true }}
                render={({ field }) => (
                  <RadioGroup {...field} onValueChange={field.onChange} value={field.value}>
                    <div className='flex items-center space-x-2'>
                      <RadioGroupItem value='steam' id='steam' />
                      <Label htmlFor='steam'>Steam</Label>
                    </div>
                    <div className='flex items-center space-x-2'>
                      <RadioGroupItem value='slm' id='slm' />
                      <Label htmlFor='slm'>SLM</Label>
                    </div>
                  </RadioGroup>
                )}
              />
              <div className='mt-4 flex justify-end gap-2'>
                <Button variant='ghost' onClick={() => resetDialog()}>
                  Vazgeç
                </Button>
                <Button type='submit'>Ekle</Button>
              </div>
            </form>
          </DialogContent>
        </Dialog>
      )}
    </>
  );
};

export default LibraryCreationDialog;
