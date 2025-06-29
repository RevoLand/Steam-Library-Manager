import { useState } from 'react';
import LibraryType from 'src/core/models/LibraryType';
import useLibraries from 'src/renderer/hooks/useLibraries';
import { Button } from '../ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '../ui/dialog';
import { Input } from '../ui/input';
import { Label } from '../ui/label';
import { RadioGroup, RadioGroupItem } from '../ui/radio-group';

const LibraryCreationDialog = () => {
  const [selectedPath, setSelectedPath] = useState('');
  const [showLabelDialog, setShowLabelDialog] = useState(false);
  const [label, setLabel] = useState('');
  const [platform, setPlatform] = useState<LibraryType>('slm');
  const { create, refreshLibraries } = useLibraries();

  const openLibraryDirectorySelector = async () => {
    const path = await window.api['select-directory']();

    if (!path) {
      return;
    }

    setSelectedPath(path);
    setShowLabelDialog(true);
  };

  const resetDialog = () => {
    setSelectedPath('');
    setLabel('');
    setShowLabelDialog(false);
    setPlatform('slm');
  };

  const createLibrary = async () => {
    try {
      await create(selectedPath, label, platform);
      refreshLibraries();

      resetDialog();
    } catch (err) {
      alert(err);
    }
  };

  return (
    <>
      <button className='text-blue-600 hover:underline text-left' onClick={() => openLibraryDirectorySelector()}>
        + Kütüphane Oluştur
      </button>
      {showLabelDialog && (
        <Dialog defaultOpen onOpenChange={resetDialog}>
          <DialogContent className='bg-white'>
            <DialogHeader>
              <DialogTitle>Yeni Kütüphane Oluştur</DialogTitle>
            </DialogHeader>
            <div className='mb-2 select-none'>
              📁 {selectedPath} <span onClick={() => openLibraryDirectorySelector()}>🔃</span>
            </div>
            <label htmlFor='label' className='text-sm font-medium'>
              Etiket (İsteğe Bağlı)
            </label>
            <Input
              id='label'
              placeholder='Örn: SSD Oyunlar, Arşiv, Retro...'
              value={label}
              onChange={(e) => setLabel(e.target.value)}
              autoFocus
            />
            <label htmlFor='label' className='text-sm font-medium'>
              Platform
            </label>
            <RadioGroup value={platform} onValueChange={(value) => setPlatform(value as LibraryType)}>
              <div className='flex items-center space-x-2'>
                <RadioGroupItem value='steam' id='steam' />
                <Label htmlFor='steam'>Steam</Label>
              </div>
              <div className='flex items-center space-x-2'>
                <RadioGroupItem value='slm' id='slm' />
                <Label htmlFor='slm'>SLM</Label>
              </div>
            </RadioGroup>
            <div className='mt-4 flex justify-end gap-2'>
              <Button variant='ghost' onClick={() => resetDialog()}>
                Vazgeç
              </Button>
              <Button onClick={createLibrary}>Ekle</Button>
            </div>
          </DialogContent>
        </Dialog>
      )}
    </>
  );
};

export default LibraryCreationDialog;
